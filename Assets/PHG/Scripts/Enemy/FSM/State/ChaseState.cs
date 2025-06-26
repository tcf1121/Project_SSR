using Unity.VisualScripting;
using UnityEngine;

namespace PHG
{
    public class ChaseState : IState
    {
        /* ───────── static ───────── */
        private static Transform sPlayer;

        public static Transform Player => sPlayer;

        /* ───────── refs ───────── */
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly JumpMove jumper;     // null → 점프 불가
        private readonly LadderClimber climber;    // null → 사다리 불가
        private readonly MonsterStatData statData;
        private readonly LayerMask groundMask;
        private readonly bool isRanged;
        bool isSpider => brain.GetComponent<SpiderTag>() != null;


        /* ───────── const / tuning ───────── */
        private const float WALL_CHECK_DIST = 0.15f;
        private const float JUMP_HEIGHT_TOL = 0.45f;
        private const float ATTACK_HEIGHT_TOL = 0.6f;
        private const float STUCK_VEL_TOL = 0.05f; // x 속도 이하면 막힘 간주
        private const float AIR_ACCEL = 8f;    // 공중 X 가속도

        /* ───────── state vars ───────── */
        private float jumpTimer;

        /* ================================================================= */
        public ChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            jumper = brain.GetComponent<JumpMove>();   // 없으면 null
            climber = brain.GetComponent<LadderClimber>();
            statData = brain.StatData;
            isRanged = brain.GetComponent<RangedTag>() != null;
            groundMask = brain.groundMask;
        }

        /* ----------------------------------------------------------------- */
        #region IState
        public void Enter()
        {
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

            rb.velocity = Vector2.zero;
            jumpTimer = 0f;
        }

        public void Tick()
        {
            if (sPlayer == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* ─── (0) 사다리 등반 중이면 Chase 로직 일시 정지 ─── */
            if (climber != null && climber.IsClimbing)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            Vector2 toPl = sPlayer.position - tf.position;
            float absDx = Mathf.Abs(toPl.x);
            float absDy = Mathf.Abs(toPl.y);
            float dist = toPl.magnitude;
            int dir = toPl.x > 0 ? 1 : -1;

            Orient(dir);

            bool grounded = IsGrounded();

            // ───────── ① 공격 시도 ─────────
            if (grounded && TryAttack(absDx, absDy))
                return;

            // ───────── ② 이동 / 점프 판단 ─────────
            bool wallAhead = Physics2D.Raycast(tf.position, Vector2.right * dir,
                                               WALL_CHECK_DIST, groundMask);
            bool needJump = grounded && jumper != null &&
                             (wallAhead || absDy > JUMP_HEIGHT_TOL);
            bool stuck = grounded && Mathf.Abs(rb.velocity.x) < STUCK_VEL_TOL;

            float jumpCd = Mathf.Max(statData.jumpCooldown, 0.45f);

            if ((needJump || stuck) && jumpTimer <= 0f && jumper != null && jumper.Ready())
            {
                float yBoost = wallAhead ? statData.jumpForce * 0.2f : 0f;
                float jumpForceY = statData.jumpForce + yBoost;

                jumper.DoJump(dir, absDy, jumpForceY, statData.jumpHorizontalFactor, jumpCd);
                jumpTimer = jumpCd;
            }
            else
            {
                // 지상 → 즉시 설정 / 공중 → 가속 보간
                float spiderBoost = (isSpider && dist < brain.Stats.ChargeRange) ? 2.0f : 1f;
                float targetX = dir * statData.moveSpeed * spiderBoost;
                if (grounded)
                    rb.velocity = new Vector2(targetX, rb.velocity.y);
                else // airborne 가속 유지
                    rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetX,
                                                                 AIR_ACCEL * Time.deltaTime),
                                              rb.velocity.y);
            }

            // ───────── ③ 추격 포기 ─────────
            if (dist > statData.chaseRange)
                brain.ChangeState(StateID.Patrol);

            if (jumpTimer > 0f) jumpTimer -= Time.deltaTime;

            //y차가 크고 사다리가 있다면 등반시도
            if (climber != null && Mathf.Abs(toPl.y) > climber.MinYThreshold)
                climber.ScanAheadAndClimb(dir);

        

        }

        public void Exit() => rb.velocity = Vector2.zero;
        #endregion

        /* ----------------------------------------------------------------- */
        #region helpers
        private bool IsGrounded()
        {
            if (jumper != null)
                return jumper.IsGrounded();

            // 점프 컴포넌트 없으면 단순 Raycast ↓ 로 대체
            return Physics2D.Raycast(tf.position, Vector2.down, 0.05f, groundMask);
        }

        private void Orient(int dir)
        {
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            tf.localScale = s;
        }

        private bool TryAttack(float absDx, float absDy)
        {
            if (isRanged && absDx <= statData.readyRange)
            {
                brain.ChangeState(StateID.Attack);
                return true;
            }
            if (!isRanged && absDx <= 1.0f && absDy < ATTACK_HEIGHT_TOL)
            {
                brain.ChangeState(StateID.Attack);
                return true;
            }
            return false;
        }

       


        #endregion
    }
}