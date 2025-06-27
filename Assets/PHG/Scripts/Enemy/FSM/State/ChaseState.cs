using Unity.VisualScripting;
using UnityEngine;

namespace PHG
{
    public class ChaseState : IState
    {
        /* ───── static ───── */
        private static Transform sPlayer;
        public static Transform Player => sPlayer;

        /* ───── refs ───── */
        readonly MonsterBrain brain;        // (멤버 변수)
        readonly Rigidbody2D rb;           // (멤버 변수)
        readonly Transform tf;           // (멤버 변수)
        readonly JumpMove jumper;       // (멤버 변수)
        readonly LadderClimber climber;      // (멤버 변수)
        readonly MonsterStatData statData;   // (멤버 변수)
        readonly LayerMask groundMask;   // (멤버 변수)
        readonly bool isRanged;     // (멤버 변수)
        bool IsSpider => brain.GetComponent<SpiderTag>() != null;

        /* ★ 사격용 */
        Transform muzzle;                     // (멤버 변수)
        float lastShot;                   // (멤버 변수)

        /* ───── tuning ───── */
        const float WALL_CHECK_DIST = 0.15f;
        const float JUMP_HEIGHT_TOL = 0.45f;
        const float ATTACK_HEIGHT_TOL = 0.6f;
        const float STUCK_VEL_TOL = 0.05f;
        const float AIR_ACCEL = 8f;

        /* ───── vars ───── */
        float jumpTimer;

        /* ================================================================ */
        public ChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            jumper = brain.GetComponent<JumpMove>();
            climber = brain.GetComponent<LadderClimber>();
            statData = brain.StatData;
            isRanged = brain.GetComponent<RangedTag>() != null;
            groundMask = brain.groundMask;

            muzzle = tf.Find("MuzzlePoint");
        }

        /* -------------------------------------------------------------- */
        #region IState
        public void Enter()
        {
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

            rb.velocity = Vector2.zero;
            jumpTimer = 0f;
            lastShot = -statData.rangedCooldown;
        }

        public void Tick()
        {
            if (sPlayer == null) { brain.ChangeState(StateID.Patrol); return; }

            /* 등반 중이면 정지 */
            if (climber != null && climber.IsClimbing) { rb.velocity = Vector2.zero; return; }

            Vector2 toPl = sPlayer.position - tf.position;
            float dist = toPl.magnitude;              // XY 거리
            int dir = toPl.x > 0 ? 1 : -1;

            Orient(dir);

            /* readyRange 안에 들어오면 Aim 상태로 전환 */
            if (isRanged && dist <= statData.readyRange)
            {
                brain.ChangeState(StateID.Attack); // RangeAttackState
                return;
            }

            /* -------- 이동 / 점프 / 추적 포기 계산 (사격 전에 위치) -------- */
            bool grounded = (jumper != null) ? jumper.IsGrounded() : Physics2D.Raycast(tf.position, Vector2.down, 0.05f, groundMask);
            bool wallAhead = Physics2D.Raycast(tf.position, Vector2.right * dir, WALL_CHECK_DIST, groundMask);
            bool needJump = grounded && jumper != null && (wallAhead || Mathf.Abs(toPl.y) > JUMP_HEIGHT_TOL);
            bool stuck = grounded && Mathf.Abs(rb.velocity.x) < STUCK_VEL_TOL;
            float jumpCd = Mathf.Max(statData.jumpCooldown, 0.45f);

            if ((needJump || stuck) && jumpTimer <= 0f && jumper?.Ready() == true)
            {
                float boostY = wallAhead ? statData.jumpForce * 0.2f : 0f;
                jumper.DoJump(dir, Mathf.Abs(toPl.y), statData.jumpForce + boostY, statData.jumpHorizontalFactor, jumpCd);
                jumpTimer = jumpCd;
            }
            else
            {
                float spiderBoost = (IsSpider && dist < brain.Stats.ChargeRange) ? 2f : 1f;
                float targetX = dir * statData.moveSpeed * spiderBoost;

                if (grounded)
                    rb.velocity = new Vector2(targetX, rb.velocity.y);
                else
                    rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetX, AIR_ACCEL * Time.deltaTime), rb.velocity.y);
            }

            /* -------- ChaseRange 벗어나면 Patrol 복귀 -------- */
            if (dist > statData.chaseRange) brain.ChangeState(StateID.Patrol);
            if (jumpTimer > 0f) jumpTimer -= Time.deltaTime;

            /* -------- Ladder Scan -------- */
            if (climber != null && Mathf.Abs(toPl.y) > climber.MinYThreshold)
                climber.ScanAheadAndClimb(dir);

            /* -------- 이동 중 사격 (velocity 유지) -------- */
            if (isRanged && dist > statData.attackRange && dist <= statData.chaseRange && Time.time - lastShot >= statData.rangedCooldown)
            {
                Shoot();
                lastShot = Time.time;
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;
        #endregion

        /* -------------------------------------------------------------- */
        #region helpers
        void Orient(int dir)
        {
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            tf.localScale = s;
        }

        void Shoot()
        {
            if (muzzle == null) return;
            Vector2 dir = (sPlayer.position - muzzle.position).normalized;
            Projectile proj = ProjectilePool.Instance.Get(statData.projectileprefab, muzzle.position);
            proj.Launch(dir);
        }
        #endregion
    }
}
