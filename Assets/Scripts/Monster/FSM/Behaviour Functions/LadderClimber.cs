using System.Collections;
using UnityEngine;


public interface IMonsterClimber
{
    bool IsClimbing { get; }
    void Init(Monster monster);
    void TryFindAndClimb(int dir);
    void UpdateClimbTimer(float dt);
}

public class LadderClimber : IMonsterClimber
{
    private float climbSpeed = 3f;
    private float alignSpeed = 4f;
    private float climbYThreshold = 0.7f;
    private float detectRadius = 0.35f;
    private LayerMask ladderMask;
    private Vector2 forwardOffset = new(0.15f, 0);

    private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
    private const float CLIMB_COOLDOWN = 0.75f;

    // 사다리 이탈 시 점프 힘 (Y축)
    private const float LADDER_EXIT_JUMP_FORCE_Y = 3.0f;
    // 사다리 이탈 시 수평 속도
    private const float LADDER_EXIT_HORIZONTAL_VELOCITY = 2.0f;

    // 사다리 상단에서 "완전히 벗어났다"고 판단할 Y 오프셋 (몬스터의 높이 등을 고려)
    private const float LADDER_TOP_EXIT_HEIGHT_OFFSET = 0.2f;

    // 사다리 이탈 시 플레이어 방향으로 강제 이동시킬 X 오프셋 (여전히 사용됨)
    private const float FORCE_EXIT_X_OFFSET = 0.3f;


    private Rigidbody2D rb;
    private Collider2D monsterCollider; // 몬스터 콜라이더 참조
    private Transform tf;
    private Monster _monster;
    private MonsterBrain brain;

    public bool IsClimbing { get; private set; }
    private float cooldownTimer;

    public float MinYThreshold => climbYThreshold;
    public Vector2 ForwardOffset => forwardOffset;
    public float DetectRadius => detectRadius;

    public void Init(Monster monster)
    {
        _monster = monster;
        brain = monster.Brain;
        rb = brain.Monster.Rigid;
        tf = brain.Monster.transform;
        monsterCollider = brain.Monster.GetComponent<Collider2D>(); // 콜라이더 참조 초기화

        var stat = brain.StatData;
        climbSpeed = stat.climbSpeed;
        forwardOffset = stat.ladderForwardOffset;
        detectRadius = stat.ladderDetectRadius;
        climbYThreshold = stat.climbYThreshold;
        ladderMask = stat.ladderMask;
    }

    public void UpdateClimbTimer(float dt)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= dt;
    }

    public void TryFindAndClimb(int dir)
    {
        if (IsClimbing || cooldownTimer > 0f) return;

        Vector2 probe = (Vector2)tf.position + forwardOffset * Mathf.Sign(tf.localScale.x);
        Collider2D col = Physics2D.OverlapCircle(probe, detectRadius, ladderMask);

#if UNITY_EDITOR
        Debug.DrawLine(tf.position, probe, Color.cyan, 1f);
        Debug.DrawLine(probe, probe + Vector2.up * detectRadius, Color.red, 1f);
        Debug.DrawLine(probe, probe + Vector2.down * detectRadius, Color.red, 1f);
        Debug.DrawLine(probe, probe + Vector2.left * detectRadius, Color.red, 1f);
        Debug.DrawLine(probe, probe + Vector2.right * detectRadius, Color.red, 1f);
#endif

        if (col == null) return;

        float yDiff = _monster.Target.position.y - tf.position.y;
        LadderBounds lb = col.GetComponentInParent<LadderBounds>();
        if (lb == null)
        {
            return;
        }

        bool goUp = yDiff > 0;
        bool canClimb = false;

        if (goUp)
        {
            if (!_monster.Brain.IsGrounded())
            {
                return;
            }
            canClimb = tf.position.y < lb.top.position.y;
        }
        else
        {
            canClimb = tf.position.y > lb.bottom.position.y + climbYThreshold;
        }

        if (!canClimb)
        {
            return;
        }

        brain.StartCoroutine(ClimbRoutine(lb, goUp));
    }

    private IEnumerator ClimbRoutine(LadderBounds lb, bool initialGoUp)
    {
        IsClimbing = true;
        int originalLayer = brain.gameObject.layer;
        int climbLayer = LayerMask.NameToLayer(CLIMBING_LAYER_NAME);
        if (climbLayer != -1) brain.gameObject.layer = climbLayer;

        rb.simulated = false; // 사다리 등반 중에는 물리 비활성화
        _monster.PlayAnim(AnimNames.Walk);

        // 사다리 중앙으로 X축 정렬
        float xMid = lb.bottom.position.x;
        while (Mathf.Abs(tf.position.x - xMid) > 0.05f)
        {
            tf.position = Vector3.MoveTowards(tf.position, new Vector3(xMid, tf.position.y, tf.position.z), alignSpeed * Time.deltaTime);
            yield return null;
        }
        tf.position = new Vector3(xMid, tf.position.y, tf.position.z);

        float tolerance = 0.05f;
        Transform playerTf = _monster.Target;

        // 몬스터 콜라이더 높이를 고려하여 사다리 상단 탈출 한계를 더 높게 잡습니다.
        float climbUpperLimit = lb.top.position.y + LADDER_TOP_EXIT_HEIGHT_OFFSET;

        while (true)
        {
            // 플레이어 타겟이 없거나, 플레이어가 감지 범위를 벗어나면 사다리에서 이탈
            if (playerTf == null || !_monster.PlayerInRange(brain.StatData.chaseRange + 1f))
            {
                Debug.Log("[ClimbRoutine] 플레이어 범위 이탈 또는 타겟 없음 → 사다리 이탈");
                break;
            }

            // 현재 위치에서 더 이상 사다리가 감지되지 않으면 사다리 이탈
            Vector2 currentCheckPos = (Vector2)tf.position + forwardOffset * Mathf.Sign(tf.localScale.x);
            Collider2D currentLadderCol = Physics2D.OverlapCircle(currentCheckPos, detectRadius, ladderMask);
            if (currentLadderCol == null)
            {
                Debug.Log("[ClimbRoutine] 사다리 이탈 (허공) → 강제 종료");
                break;
            }

            float yDiffToPlayer = playerTf.position.y - tf.position.y;
            bool goUp = yDiffToPlayer > 0;

            bool atTopLimit = tf.position.y >= climbUpperLimit - tolerance;
            bool atBottomLimit = !goUp && tf.position.y <= lb.bottom.position.y + tolerance;

            // 사다리 상단 도달 또는 X축 이탈 거리 초과 시 이탈 준비
            if (atTopLimit || (Mathf.Abs(playerTf.position.x - tf.position.x) > 2.5f && _monster.Brain.IsGrounded()))
            {
                Debug.Log("[ClimbRoutine] 사다리 상단 도달 또는 X축 이탈 거리 초과 → 이탈 시도.");
                // 여기서 사다리 이탈 로직으로 바로 넘어갑니다.
                break;
            }
            else if (atBottomLimit)
            {
                Debug.Log("[ClimbRoutine] 사다리 하단 도달. 등반 종료.");
                break;
            }
            else
            {
                float targetY = playerTf.position.y;
                targetY = Mathf.Clamp(targetY, lb.bottom.position.y, climbUpperLimit);

                float newY = Mathf.MoveTowards(tf.position.y, targetY, climbSpeed * Time.deltaTime);
                tf.position = new Vector3(tf.position.x, newY, tf.position.z);
            }

            yield return null;
        }

        // --- 사다리 이탈 최종 처리 ---
        // 콜라이더를 잠시 비활성화하여 강제 점프/이동 시 지형과의 충돌 방지
        if (monsterCollider != null) monsterCollider.enabled = false;
        rb.simulated = false; // 물리 시뮬레이션은 여전히 비활성화 상태

        if (playerTf != null)
        {
            int playerXDir = (int)Mathf.Sign(playerTf.position.x - tf.position.x);
            // 사다리에서 살짝 위로 띄우고 플레이어 방향으로 밀어내는 위치 조정
            Vector3 exitPos = tf.position;
            exitPos.y += LADDER_TOP_EXIT_HEIGHT_OFFSET * 0.5f; // 사다리 위로 좀 더 명확하게 띄움
            exitPos.x += playerXDir * FORCE_EXIT_X_OFFSET; // 플레이어 방향으로 살짝 이동
            tf.position = exitPos;
        }

        yield return null; // 위치 변경 적용을 위해 한 프레임 대기

        // 콜라이더 재활성화 및 물리 시뮬레이션 활성화
        if (monsterCollider != null) monsterCollider.enabled = true;
        rb.simulated = true;
        rb.velocity = Vector2.zero; // 모든 속도 초기화

        // 플레이어 방향으로 점프 및 수평 추진력 부여
        if (playerTf != null)
        {
            int playerXDir = (int)Mathf.Sign(playerTf.position.x - tf.position.x);
            rb.velocity = new Vector2(playerXDir * LADDER_EXIT_HORIZONTAL_VELOCITY, LADDER_EXIT_JUMP_FORCE_Y);
            Debug.Log($"[ClimbRoutine] 사다리 이탈 후 점프 및 수평 추진력 적용: Velocity X={rb.velocity.x:F2}, Y={rb.velocity.y:F2}");
        }

        // --- 사다리 이탈 최종 처리 끝 ---

        brain.gameObject.layer = originalLayer;
        IsClimbing = false;
        cooldownTimer = CLIMB_COOLDOWN;

        Debug.Log("[ClimbRoutine] 사다리 등반 코루틴 종료. Chase 상태로의 전환은 MonsterBrain에 맡김.");
        // brain.ChangeState(StateID.Chase); // 이 부분을 제거합니다!
    }
}