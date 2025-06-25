using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;


namespace PHG
{

    /// <summary>
    /// State – Chase
    /// 점프 → 2회 실패 후 사다리 시도 → 평지 추격 → 지면에서만 Attack
    /// </summary>
    public class ChaseState : IState
    {
        /* ───── static ───── */
        static Transform sPlayer;   // 모든 모델스터가 공유하는 플레이어 캐시

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
        const float GROUND_DEADZONE = 0.05f;  // 너무 가치면 정지
        const int MAX_JUMP_FAILS = 2;      // 점프 실패 허용 횟수
        const float ATTACK_HEIGHT_TOL = 0.6f;   // 공격 허용 Y 차이
        const float GROUND_CHECK_DIST = 0.15f;  // 발림 지면 체크 거리

        /* ───── runtime ───── */
        int jumpFailCounter;
        int chaseDir; // 현재 추가 방향 (1: 우, -1: 왼)
        bool wasGrounded;

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

            float dx = sPlayer.position.x - tf.position.x;
            chaseDir = dx > 0 ? 1 : -1;
            tf.localScale = new Vector3(chaseDir, 1f, 1f);
        }

        public void Tick()
        {
            if (sPlayer == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }


            if (climber != null && climber.IsClimbing) return;

            float dx = sPlayer.position.x - tf.position.x;
            float dy = sPlayer.position.y - tf.position.y;
            float absDx = Mathf.Abs(dx);
            float distToPlayer = Vector2.Distance(tf.position, sPlayer.position);
            int dir = dx > 0 ? 1 : -1;

            // 점프 중에는 방향 고정 (회전 X)
            if (jumper == null || !jumper.IsMidJump)
            {
                chaseDir = dx > 0 ? 1 : -1;
                tf.localScale = new Vector3(chaseDir, 1f, 1f);
            }

            bool grounded = Physics2D.Raycast(tf.position + Vector3.down * 0.05f,
                                              Vector2.down, GROUND_CHECK_DIST,
                                              brain.groundMask);

            float attackDist = brain.Stats.AttackRange;
            if (grounded && absDx <= attackDist && Mathf.Abs(dy) < ATTACK_HEIGHT_TOL)
            {
                rb.velocity = Vector2.zero;
                brain.ChangeState(StateID.Attack);
                return;
            }
            if (grounded && distToPlayer <= brain.Stats.ChaseRange)
            {
                dx = sPlayer.position.x - tf.position.x;
                chaseDir = dx > 0 ? 1 : -1;
                tf.localScale = new Vector3(chaseDir, 1f, 1f);

                if (absDx > GROUND_DEADZONE)
                    rb.velocity = new Vector2(chaseDir * brain.Stats.MoveSpeed, rb.velocity.y);
                else
                    rb.velocity = Vector2.zero;

                return; // ✅ 여기서 마무리하면 나머지 점프/벽점프 로직 건드리지 않음
            }
            if (grounded && !wasGrounded)
            {
                float dx2 = sPlayer.position.x - tf.position.x;
                chaseDir = dx2 > 0 ? 1 : -1;
                tf.localScale = new Vector3(chaseDir, 1f, 1f);
                if (Vector2.Distance(tf.position, sPlayer.position) <= brain.Stats.ChaseRange)
                    rb.velocity = new Vector2(chaseDir * brain.Stats.MoveSpeed, rb.velocity.y);
            }
            wasGrounded = grounded;

            // ✅ 플레이어가 ChaseRange 밖에 있을 경우만 Patrol 전환
            if (Vector2.Distance(tf.position, sPlayer.position) > brain.Stats.ChaseRange)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }
            if (jumper == null)
            {
                RaycastHit2D groundHit = Physics2D.Raycast(tf.position + Vector3.down * 0.1f, Vector2.down, 1f, brain.groundMask);
                if (groundHit.collider != null)
                {
                    Bounds bounds = groundHit.collider.bounds;
                    float left = bounds.min.x;
                    float right = bounds.max.x;

                    float playerX = sPlayer.position.x;
                    if (playerX < left || playerX > right)
                    {
                        rb.velocity = Vector2.zero;
                        return;
                    }
                }
            }
            // ✅ 점프 시도 (플래폼 위로)
            if (jumper != null)
            {
                if (Mathf.Abs(dy) > Y_JUMP_THRESHOLD)
                {
                    bool jumped = jumper.TryJumpToPlatformAbove(chaseDir, sPlayer.position);
                    if (jumped)
                    {
                        jumpFailCounter = 0;
                        return;
                    }
                    jumpFailCounter++;
                }
            }

            // ✅ 벽 점프
            bool wallAhead = Physics2D.Raycast(tf.position + Vector3.up * 0.1f,
                                                Vector2.right * chaseDir, 0.25f, brain.groundMask);

            if (wallAhead && jumper != null)
            {
                bool jumped = jumper.TryJumpWallOrPlatform(chaseDir, sPlayer.position.y);
                if (jumped)
                {
                    jumpFailCounter = 0;
                    return;
                }
                jumpFailCounter++;
            }

            // ✅ 사다리는 조건 마지막 시 자동 (별도 전환 없음)
            bool ladderGap = Mathf.Abs(dy) >= Y_LADDER_THRESHOLD;
            bool ladderCond = ladderGap &&
                              jumpFailCounter >= MAX_JUMP_FAILS &&
                              Mathf.Abs(dx) <= X_LADDER_TOL &&
                              climber != null;

            // ✅ 평지 추적
            if (absDx > GROUND_DEADZONE)
                rb.velocity = new Vector2(chaseDir * brain.Stats.MoveSpeed, rb.velocity.y);
            else
                rb.velocity = Vector2.zero;

            // ✅ 점프 중에는 이동만 유지
            if (jumper != null && jumper.IsMidJump)
            {
                float dx3 = sPlayer.position.x - tf.position.x;
                int dir2 = dx3 > 0 ? 1 : -1;
                chaseDir = dir2;
                tf.localScale = new Vector3(chaseDir, 1f, 1f);
                rb.velocity = new Vector2(chaseDir * brain.Stats.MoveSpeed, rb.velocity.y);
                return;
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;
    }
}
