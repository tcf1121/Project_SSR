using System.Collections;
using UnityEngine;


public interface IMonsterClimber
{
    bool IsClimbing { get; }
    void Init(MonsterBrain brain);
    void TryFindAndClimb(int dir, Vector2 playerPos);
    void UpdateClimbTimer(float dt);
}

public class LadderClimber : IMonsterClimber
{
    private float climbSpeed = 3f;
    private float alignSpeed = 4f;
    private float jumpAwayForce = 3f;
    private float climbYThreshold = 0.7f;
    private LayerMask ladderMask;
    private float detectRadius = 0.35f;
    private Vector2 forwardOffset = new(0.15f, 0);

    private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
    private const float CLIMB_COOLDOWN = 0.75f;

    private Rigidbody2D rb;
    private Transform tf;
    private MonsterBrain brain;

    public bool IsClimbing { get; private set; }
    private float cooldownTimer;

    public float MinYThreshold => climbYThreshold;
    public Vector2 ForwardOffset => forwardOffset;
    public float DetectRadius => detectRadius;

    public void Init(MonsterBrain brain)
    {
        this.brain = brain;
        rb = brain.Monster.Rigid;
        tf = brain.Monster.transform;

        var stat = brain.StatData;
        climbSpeed = stat.climbSpeed;
        forwardOffset = stat.ladderForwardOffset;
        detectRadius = stat.ladderDetectRadius;
        climbYThreshold = stat.climbYThreshold;
        ladderMask = stat.ladderMask;

        // Debug.Log($"[LadderClimber] Init 완료: offset={forwardOffset}, radius={detectRadius}, threshold={climbYThreshold}");
    }

    public void UpdateClimbTimer(float dt)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= dt;
    }

    public void TryFindAndClimb(int dir, Vector2 playerPos)
    {
        if (IsClimbing || cooldownTimer > 0f) return;

        Vector2 probe = (Vector2)tf.position + forwardOffset * dir;
        Collider2D col = Physics2D.OverlapCircle(probe, detectRadius, ladderMask);

#if UNITY_EDITOR
        Debug.DrawLine(tf.position, probe, Color.cyan, 1f);
#endif


        if (col == null) return;

        float yDiff = playerPos.y - tf.position.y;

        // 예외 처리: 플레이어가 아래에 있고 yDiff가 작지만 여전히 bottom에 가까울 때는 내려가야 함
        if (Mathf.Abs(yDiff) < climbYThreshold)
        {
            LadderBounds tmpLB = col.GetComponentInParent<LadderBounds>();
            if (tmpLB != null)
            {
                bool goDownBecausePlayerClearlyBelow = (yDiff < 0 && tf.position.y > tmpLB.bottom.position.y + 0.2f);
                if (!goDownBecausePlayerClearlyBelow) return;
            }
            else
            {
                return;
            }
        }

        LadderBounds lb = col.GetComponentInParent<LadderBounds>();
        if (lb == null)
        {
            //  Debug.LogWarning("[LadderClimber] LadderBounds 없음");
            return;
        }

        brain.StartCoroutine(ClimbRoutine(lb, yDiff > 0f, playerPos));
    }

    private IEnumerator ClimbRoutine(LadderBounds lb, bool goUp, Vector2? playerPos)
    {
        IsClimbing = true;

        int originalLayer = brain.gameObject.layer;
        int climbLayer = LayerMask.NameToLayer(CLIMBING_LAYER_NAME);
        if (climbLayer != -1) brain.gameObject.layer = climbLayer;

        rb.simulated = false;

        float xMid = lb.bottom.position.x;
        tf.position = new Vector3(xMid, tf.position.y, tf.position.z);
        yield return null;

        float tolerance = 0.05f;
        Transform playerTf = GameObject.FindWithTag("Player")?.transform;

        while (true)
        {
            if (playerTf == null) break;

            float targetY = playerTf.position.y;

            // --- 1단계: 목표에 도달할 때까지 이동 ---
            while (Mathf.Abs(tf.position.y - targetY) > tolerance)
            {
                if (playerTf == null) break;

                float yDiff = playerTf.position.y - tf.position.y;
                float xDiff = Mathf.Abs(playerTf.position.x - tf.position.x);

                if (xDiff > 1.5f)
                {
                    //Debug.Log("[ClimbRoutine] 플레이어 이탈 → 점프 추격");
                    goto Exit;
                }

                // 방향 바뀌면 다시 바깥 루프로
                if ((goUp && yDiff < -0.5f) || (!goUp && yDiff > 0.5f))
                {
                    goUp = yDiff > 0;
                    // Debug.Log("[ClimbRoutine] 방향 반전 → 재진입");
                    break;
                }

                targetY = playerTf.position.y;
                float newY = Mathf.MoveTowards(tf.position.y, targetY, climbSpeed * Time.deltaTime);
                tf.position = new Vector3(tf.position.x, newY, tf.position.z);
                yield return null;
            }

            // --- 2단계: 도달 후 플레이어 상태 대기 ---
            while (true)
            {
                if (playerTf == null) break;

                float yDiff = playerTf.position.y - tf.position.y;
                float xDiff = Mathf.Abs(playerTf.position.x - tf.position.x);

                if (xDiff > 1.5f || Mathf.Abs(yDiff) > 1.5f)
                {
                    Debug.Log("[ClimbRoutine] 플레이어 멀어짐 → 점프 추격");
                    goto Exit;
                }

                // 방향 바뀌면 1단계로 다시 이동
                if ((goUp && yDiff < -0.5f) || (!goUp && yDiff > 0.5f))
                {
                    goUp = yDiff > 0;
                    Debug.Log("[ClimbRoutine] 방향 반전 (정지 후) → 재진입");
                    break;
                }

                bool playerGrounded = Physics2D.OverlapCircle(
                    playerTf.position + Vector3.down * 0.1f,
                    0.1f,
                    LayerMask.GetMask("Ground", "Platform"));

                if (playerGrounded)
                {
                    Debug.Log("[ClimbRoutine] 플레이어 착지 → 사다리 이탈");
                    goto Exit;
                }

                yield return null;
            }
        }

    Exit:
        rb.simulated = true;
        brain.gameObject.layer = originalLayer;
        IsClimbing = false;
        cooldownTimer = 0.5f;

        if (playerTf != null && Mathf.Abs(playerTf.position.x - tf.position.x) > 1.5f)
        {
            int hDir = playerTf.position.x > tf.position.x ? 1 : -1;
            float yGap = playerTf.position.y - tf.position.y;
            if (brain.CanJump)
            {
                brain.PerformJump(hDir, Mathf.Abs(yGap), brain.StatData.jumpForce,
                                  brain.StatData.jumpHorizontalFactor,
                                  brain.StatData.jumpCooldown);
            }
        }
        else
        {
            brain.ChangeState(StateID.Chase);
        }
    }
}
