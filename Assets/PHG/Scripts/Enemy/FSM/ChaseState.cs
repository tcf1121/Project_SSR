using PHG;
using UnityEngine;

/// <summary>
/// State – Chase
/// 점프 → 2회 실패 후 사다리 시도 → 평지 추격 → 지면에서만 Attack
/// </summary>
public class ChaseState : IState
{
    /* ───── static ───── */
    static Transform sPlayer;   // 모든 몬스터가 공유하는 플레이어 캐시

    /* ───── refs ───── */
    readonly MonsterBrain brain;
    readonly Rigidbody2D rb;
    readonly Transform tf;
    readonly JumpMove jumper;
    readonly LadderClimber climber;

    /* ───── tunables ───── */
    const float Y_JUMP_THRESHOLD = 0.9f;   // 점프 필요 높이
    const float Y_LADDER_THRESHOLD = 1.8f;   // 사다리 고려 높이
    const float X_LADDER_TOL = 0.25f;  // 사다리 시도 X 오차
    const float GROUND_DEADZONE = 0.05f;  // 너무 가까우면 정지
    const int MAX_JUMP_FAILS = 2;      // 점프 실패 허용 횟수
    const float ATTACK_HEIGHT_TOL = 0.6f;   // 공격 허용 Y 차이
    const float GROUND_CHECK_DIST = 0.15f;  // 발밑 지면 체크 거리

    /* ───── runtime ───── */
    int jumpFailCounter;

    public ChaseState(MonsterBrain brain)
    {
        this.brain = brain;
        rb = brain.GetComponent<Rigidbody2D>();
        tf = brain.transform;
        jumper = brain.GetComponent<JumpMove>();
        climber = brain.GetComponent<LadderClimber>();
    }

    public void Enter()
    {
        if (sPlayer == null)
            sPlayer = GameObject.FindWithTag("Player")?.transform;

        rb.velocity = Vector2.zero;
        jumpFailCounter = 0;
    }

    public void Tick()
    {
        /* 0) 플레이어 유무 */
        if (sPlayer == null)
        {
            brain.ChangeState(StateID.Patrol);
            return;
        }

        /* 1) 사다리 등·하강 중이면 대기 */
        if (climber != null && climber.IsClimbing) return;

        /* 2) 방향·거리 계산 */
        float dx = sPlayer.position.x - tf.position.x;
        float dy = sPlayer.position.y - tf.position.y;
        float absDx = Mathf.Abs(dx);
        int dir = dx > 0 ? 1 : -1;
        tf.localScale = new Vector3(dir, 1f, 1f);

        /* 3) 지면&공격 판정 */
        bool grounded = Physics2D.Raycast(tf.position + Vector3.down * 0.05f,
                                          Vector2.down, GROUND_CHECK_DIST,
                                          brain.groundMask);

        if (grounded && absDx <= brain.Stats.MoveSpeed && Mathf.Abs(dy) < ATTACK_HEIGHT_TOL)
        {
            rb.velocity = Vector2.zero;
            brain.ChangeState(StateID.Attack);
            return;
        }

        /* 4) 추적 포기 */
        if (absDx > brain.Stats.MoveSpeed)
        {
            rb.velocity = Vector2.zero;
            brain.ChangeState(StateID.Patrol);
            return;
        }

        /* 5) 점프 시도 */


        bool needVertical = Mathf.Abs(dy) > Y_JUMP_THRESHOLD;
        bool wallAhead = Physics2D.Raycast(tf.position + Vector3.up * 0.1f,
                                              Vector2.right * dir, 0.25f, brain.groundMask);

        if (Mathf.Abs(dy) > Y_JUMP_THRESHOLD && jumper != null)
        {
            bool jumped = jumper.TryJumpToPlatformAbove(dir, sPlayer.position);
            if (jumped)
            {
                jumpFailCounter = 0;
                return;
            }
            jumpFailCounter++;
        }

        // 2. 앞에 벽이 있을 경우에만 벽 점프 시도
        if (wallAhead && jumper != null)
        {
            bool jumped = jumper.TryJumpWallOrPlatform(dir, sPlayer.position.y);
            if (jumped)
            {
                jumpFailCounter = 0;
                return;
            }
            jumpFailCounter++;
        }
        /* 6) 사다리 시도 */
        bool ladderGap = Mathf.Abs(dy) >= Y_LADDER_THRESHOLD;
        bool ladderCond = ladderGap &&
                          jumpFailCounter >= MAX_JUMP_FAILS &&
                          Mathf.Abs(dx) <= X_LADDER_TOL &&
                          climber != null;

        if (ladderCond && climber.TryFindAndClimb(sPlayer.position))
        {
            rb.velocity = Vector2.zero;
            jumpFailCounter = 0;
            return;                     // 등·하강 시작
        }

        /* 7) 평지 추적 */
        if (absDx > GROUND_DEADZONE)
            rb.velocity = new Vector2(dir * brain.Stats.MoveSpeed, rb.velocity.y);
        else
            rb.velocity = Vector2.zero;
        
        /*8) 점프 중이면 행동금지 */
        if (jumper.IsMidJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y); // 기존 속도 유지
            return;
        }
    }

    public void Exit() => rb.velocity = Vector2.zero;
}