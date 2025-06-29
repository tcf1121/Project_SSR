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
        readonly MonsterBrain brain;
        readonly Rigidbody2D rb;
        readonly Transform tf;
        readonly JumpMove jumper;
        readonly LadderClimber climber;
        readonly MonsterStatData statData;
        readonly LayerMask groundMask;
        readonly bool isRanged;
        bool IsSpider => brain.GetComponent<SpiderTag>() != null;

        // 새로 추가된 벽 감지 센서
        private readonly Transform wallSensor;

        /* ★ 사격용 */
        Transform muzzle;
        float lastShot;

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
            jumper = brain.GetComponent<JumpMove>(); // JumpMove 컴포넌트가 없을 수 있으므로 null일 가능성 있음
            climber = brain.GetComponent<LadderClimber>(); // LadderClimber 컴포넌트가 없을 수 있으므로 null일 가능성 있음
            statData = brain.StatData;
            isRanged = brain.GetComponent<RangedTag>() != null;
            groundMask = brain.groundMask;

            // 새로 추가된 wallSensor 할당
            this.wallSensor = brain.wallSensor;

            if (jumper != null && statData != null)
                jumper.maxJumpYDiffForAdjustment = statData.maxClimbableHeight;

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

            /* 등반 중이면 정지 (이 로직은 모든 다른 이동 로직보다 우선) */
            if (climber != null && climber.IsClimbing) { rb.velocity = Vector2.zero; return; }

            Vector2 toPl = sPlayer.position - tf.position;
            float dist = toPl.magnitude; // XY 거리
            int dir = toPl.x > 0 ? 1 : -1;

            Orient(dir);

            /* readyRange 안에 들어오면 Aim 상태로 전환 */
            if (isRanged && dist <= statData.readyRange)
            {
                brain.ChangeState(StateID.Attack); // RangeAttackState
                return;
            }

            /* -------- 이동 / 점프 / 추적 포기 계산 (사격 전에 위치) -------- */
            // JumpMove 컴포넌트가 없을 경우를 대비한 기존 로직 유지 (안정성)
            // 지면 감지용 Raycast 대체 로직을 brain.sensor (ground checker)를 활용하도록 수정
            bool grounded = (jumper != null) ? jumper.IsGrounded() : Physics2D.Raycast(brain.sensor.position, Vector2.down, 0.05f, groundMask);

            // wallSensor (전용 벽 감지 오브젝트)를 사용하여 벽 감지
            bool wallAhead = Physics2D.Raycast(wallSensor.position, Vector2.right * dir, WALL_CHECK_DIST, groundMask);
            bool targetAbove = toPl.y > 0f;

            // AI가 점프해야 한다고 판단하는 조건 (기존 needJump 로직)
            bool needJump = grounded && jumper != null && targetAbove && (wallAhead && Mathf.Abs(toPl.y) > JUMP_HEIGHT_TOL);

            bool stuck = grounded && Mathf.Abs(rb.velocity.x) < STUCK_VEL_TOL;
            float jumpCd = Mathf.Max(statData.jumpCooldown, 0.45f);

            // --- 플랫폼 가장자리 감지 로직 (이동 로직 직전에 계산) ---
            bool edgeAhead = false;
            // 점프 또는 사다리 타기 컴포넌트가 없는 몬스터에게만 가장자리 정지 로직을 적용할지 결정
            bool appliesEdgeStop = (jumper == null && climber == null);

            if (grounded && appliesEdgeStop)
            {
                float checkOffset = brain.GetComponent<Collider2D>().bounds.extents.x + 0.05f;
                // 가장자리 감지에도 ground checker (brain.sensor) 위치를 활용
                Vector2 checkOrigin = (Vector2)brain.sensor.position + Vector2.right * dir * checkOffset;
                edgeAhead = !Physics2D.Raycast(checkOrigin, Vector2.down, 0.1f, groundMask);
            }
            // ------------------------------------

            // 메인 이동 및 점프 로직 (기존 블록)
            // AI가 점프 필요하다고 판단했거나 갇혔고, JumpMove가 존재하며 물리적으로 점프할 준비가 되어있을 때
            if (jumper != null && (needJump || (stuck && targetAbove)) && jumper.ReadyToPerformJump()) // jumper.ReadyToPerformJump()와 jumper null 체크 추가
            {
                float boostY = wallAhead ? statData.jumpForce * 0.2f : 0f;
                jumper.DoJump(dir, Mathf.Abs(toPl.y), statData.jumpForce + boostY, statData.jumpHorizontalFactor, jumpCd);
                jumpTimer = jumpCd;
            }
            else // 점프하지 않는 일반적인 수평 이동 상황
            {
                // **수정된 라인**: brain.Stats.ChargeRange로 변경
                float spiderBoost = (IsSpider && dist < brain.Stats.ChargeRange) ? 2f : 1f;
                float targetX = dir * statData.moveSpeed * spiderBoost;

                if (grounded)
                {
                    // 가장자리에 도달했고, 가장자리에서 멈춰야 하는 몬스터일 경우 정지
                    if (edgeAhead && appliesEdgeStop)
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y); // 가장자리에서 멈춤
                    }
                    else // 가장자리가 아니거나, 가장자리에서 멈출 필요 없는 몬스터 (점프/사다리 타기 가능 몬스터)는 계속 이동
                    {
                        rb.velocity = new Vector2(targetX, rb.velocity.y);
                    }
                }
                else // 공중에 있을 때
                {
                    rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetX, AIR_ACCEL * Time.deltaTime), rb.velocity.y);
                }
            }

            /* -------- ChaseRange 벗어나면 Patrol 복귀 -------- */
            if (dist > statData.chaseRange) brain.ChangeState(StateID.Patrol);
            if (jumpTimer > 0f) jumpTimer -= Time.deltaTime;

            /* -------- Ladder Scan -------- */
            // 사다리 스캔 로직은 자체적으로 rb.velocity를 제어하므로, 기존 위치에 유지 (중복 아님)
            if (climber != null && Mathf.Abs(toPl.y) > climber.MinYThreshold)
                climber.ScanAheadAndClimb(dir);

            /* -------- 이동 중 사격 (velocity 유지) -------- */
            if (isRanged && dist > statData.attackRange && dist <= statData.chaseRange && Time.time - lastShot >= statData.rangedCooldown)
            {
                Shoot(); // 이 메서드는 클래스 내부에 잘 정의되어 있습니다.
                lastShot = Time.time;
            }
        }

        public void Exit() => rb.velocity = Vector2.zero; //
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
            proj.Launch(dir, statData.projectileSpeed);
        }
        #endregion
    }
}