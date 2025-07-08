using System.Collections;
using UnityEngine;

public interface IMonsterClimber
{
    bool IsClimbing { get; }
    void Init(MonsterBrain brain);
    bool TryFindAndClimb(int dir, Vector2 playerPos);
}

public class LadderClimber : IMonsterClimber
{
    private Monster _monster;
    private Rigidbody2D rb;
    private Transform tf;
    private MonsterBrain brain;
    private Collider2D monsterCollider;

    public bool IsClimbing { get; private set; }
    private float cooldownTimer;

    // MonsterStatEntry에서 가져온 설정값들
    private float climbYThreshold;
    private float ladderDetectRadius;
    private float climbSpeed;
    private float moveSpeed;

    private float ladderTopExitHeightOffset;
    private const string CLIMBING_LAYER_NAME = "MonsterClimbing";
    private const float CLIMB_COOLDOWN = 1.0f;

    public void Init(MonsterBrain brain)
    {
        this.brain = brain;
        _monster = brain.Monster;
        rb = _monster.Rigid;
        tf = _monster.transform;
        monsterCollider = _monster.GetComponent<Collider2D>();

        var stat = brain.StatData;
        climbYThreshold = stat.climbYThreshold;
        ladderDetectRadius = stat.ladderDetectRadius;
        climbSpeed = stat.climbSpeed;
        moveSpeed = stat.moveSpeed;

        if (monsterCollider)
        {
            ladderTopExitHeightOffset = monsterCollider.bounds.extents.y + 0.1f;
        }
    }

    public bool TryFindAndClimb(int dir, Vector2 playerPos)
    {
        if (IsClimbing || cooldownTimer > 0f) return false;
        if (Mathf.Abs(playerPos.y - tf.position.y) < climbYThreshold) return false;

        Collider2D[] cols = Physics2D.OverlapCircleAll(tf.position, ladderDetectRadius, brain.StatData.ladderMask);
        if (cols.Length == 0) return false;

        LadderBounds lb = cols[0].GetComponentInParent<LadderBounds>();
        if (lb == null) return false;

        brain.StartCoroutine(NavigateAndClimbRoutine(lb));
        return true;
    }

    private IEnumerator NavigateAndClimbRoutine(LadderBounds lb)
    {
        // --- 1. 등반 준비: 물리 비활성화 ---
        IsClimbing = true;
        Debug.Log($"[{_monster.name}] 등반 시작. 물리 비활성화.");

        bool originalIsTrigger = monsterCollider.isTrigger;
        float originalGravityScale = rb.gravityScale;

        // ⭐ 요청하신 대로 트리거로 전환하고 물리 영향을 받지 않도록 설정
        monsterCollider.isTrigger = true;
        rb.isKinematic = true;
        rb.velocity = Vector2.zero;

        // --- 2. 사다리 입구로 수평 이동 (Transform 제어) ---
        float ladderEntryX = lb.bottom.position.x;
        while (Mathf.Abs(tf.position.x - ladderEntryX) > 0.1f)
        {
            int dirToLadder = (ladderEntryX > tf.position.x) ? 1 : -1;
            // ⭐ Velocity 대신 Transform.Translate 사용
            tf.Translate(new Vector2(dirToLadder * moveSpeed * Time.deltaTime, 0));

            if (!_monster.PlayerInRange(brain.StatData.chaseRange * 1.2f))
            {
                Debug.Log($"[{_monster.name}] 이동 중 이탈. 등반 취소.");
                yield return TeardownPhysics(originalIsTrigger, originalGravityScale);
                yield break;
            }
            yield return null;
        }

        // --- 3. 수직 등반 (Transform 제어) ---
        Debug.Log($"[{_monster.name}] 사다리 앞 도착. 수직 등반 시작.");
        Vector2 finalExitPoint = tf.position; // 탈출 지점 초기화
        bool exited = false;

        while (!exited)
        {
            Vector2 playerPos = _monster.Target.position;
            bool goUp = playerPos.y > tf.position.y;

            // ⭐ Velocity 대신 Transform.Translate 사용
            tf.Translate(new Vector2(0, (goUp ? 1 : -1) * climbSpeed * Time.deltaTime));

            // 탈출 조건 확인
            if (goUp && tf.position.y >= lb.top.position.y)
            {
                finalExitPoint = new Vector2(lb.top.position.x, lb.top.position.y + ladderTopExitHeightOffset);
                exited = true;
            }
            else if (!goUp && tf.position.y <= lb.bottom.position.y)
            {
                finalExitPoint = lb.bottom.position;
                exited = true;
            }
            else if (!_monster.PlayerInRange(brain.StatData.chaseRange * 1.1f))
            {
                finalExitPoint = tf.position;
                exited = true;
            }

            yield return null;
        }

        // --- 4. 등반 종료: 물리 복원 ---
        tf.position = finalExitPoint;
        yield return TeardownPhysics(originalIsTrigger, originalGravityScale);
    }

    /// <summary>
    /// 비활성화했던 물리 관련 속성을 원래대로 되돌리는 공통 코루틴
    /// </summary>
    private IEnumerator TeardownPhysics(bool wasTrigger, float gravityScale)
    {
        Debug.Log($"[{_monster.name}] 등반 종료. 물리 활성화.");

        // ⭐ 물리 상태 원상 복구
        rb.isKinematic = false;
        monsterCollider.isTrigger = wasTrigger;
        rb.gravityScale = gravityScale;

        // 부드러운 추격 재개를 위한 초기 속도 부여
        int dirToPlayer = (int)Mathf.Sign(_monster.Target.position.x - tf.position.x);
        rb.velocity = new Vector2(dirToPlayer * 2f, 0.5f);

        IsClimbing = false;
        cooldownTimer = CLIMB_COOLDOWN;
        if (brain.gameObject.activeInHierarchy) brain.StartCoroutine(CooldownRoutine());

        yield return null;
    }

    private IEnumerator CooldownRoutine()
    {
        float t = 0;
        while (t < CLIMB_COOLDOWN)
        {
            cooldownTimer = CLIMB_COOLDOWN - t;
            t += Time.deltaTime;
            yield return null;
        }
        cooldownTimer = 0f;
    }
}