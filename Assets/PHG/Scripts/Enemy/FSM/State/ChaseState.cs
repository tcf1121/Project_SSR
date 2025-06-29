using UnityEngine;
// using Unity.VisualScripting; // 사용되지 않는 것 같으니 제거 고려

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
        readonly MonsterStatEntry statData;
        readonly LayerMask groundMask;
        readonly bool isRanged;
        bool IsSpider => brain.GetComponent<SpiderTag>() != null;

        private readonly Transform wallSensor;

        /* ★ 사격용 */
        Transform muzzle;
        float lastShot; // 이 변수는 Range Monster의 경우 AttackState로 이동하거나,
                        // ChaseState에서도 사격을 허용할 경우 사용됩니다.

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
            rb = brain.rb;
            tf = brain.tf;
            jumper = brain.GetComponent<JumpMove>();
            climber = brain.GetComponent<LadderClimber>();
            statData = brain.statData;
            groundMask = brain.groundMask;
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

            Vector2 toPl = (Vector2)sPlayer.position - (Vector2)tf.position;
            float dist = toPl.magnitude;
            int dir = (toPl.x > 0) ? 1 : -1;

            // 점프 중이 아닐 때만 방향 전환을 허용합니다.
            if (jumper == null || !jumper.IsMidJump)
            {
                Orient(dir);
            }

            /* readyRange 안에 들어오면 Attack 상태로 전환 */
            // Range Attack Monster는 이 조건에서 AttackState로 전환되어 거기서 공격해야 합니다.
            if (isRanged && dist <= statData.attackRange)
            {
                brain.ChangeState(StateID.Attack);
                return;
            }
            // 근거리 몬스터는 attackRange 안에 들어오면 AttackState로 전환됩니다.
            else if (!isRanged && dist <= statData.attackRange)
            {
                brain.ChangeState(StateID.Attack);
                return;
            }


            /* -------- 이동 / 점프 / 추적 포기 계산 (사격 전에 위치) -------- */
            bool grounded = (jumper != null) ? jumper.IsGrounded() : Physics2D.Raycast(brain.sensor.position, Vector2.down, 0.05f, groundMask);

            RaycastHit2D wallCheck = Physics2D.Raycast(wallSensor.position, tf.right * Mathf.Sign(tf.localScale.x), WALL_CHECK_DIST, groundMask);
            bool wallAhead = wallCheck.collider != null;

            bool targetAbove = toPl.y > 0f;

            // AI가 점프해야 한다고 판단하는 조건:
            // 플레이어가 몬스터보다 위에 있고 (targetAbove),
            // (벽이 앞에 있고 && 플레이어 Y가 JUMP_HEIGHT_TOL보다 높을 때)
            bool needJump = grounded && jumper != null && targetAbove && (wallAhead && Mathf.Abs(toPl.y) > JUMP_HEIGHT_TOL);

            // stuck 정의: 땅에 있고, 벽이 앞에 있으며, 수평 속도가 STUCK_VEL_TOL 미만일 때
            bool stuck = grounded && wallAhead && Mathf.Abs(rb.velocity.x) < STUCK_VEL_TOL;
            float jumpCd = Mathf.Max(statData.jumpCooldown, 0.45f);

            // --- 플랫폼 가장자리 감지 로직 ---
            bool edgeAhead = false;
            bool appliesEdgeStop = (jumper == null && climber == null);

            if (grounded && appliesEdgeStop)
            {
                float checkOffset = brain.GetComponent<Collider2D>().bounds.extents.x + 0.05f;
                Vector2 checkOrigin = (Vector2)brain.sensor.position + Vector2.right * dir * checkOffset;
                edgeAhead = !Physics2D.Raycast(checkOrigin, Vector2.down, 0.1f, groundMask);
            }
            // ------------------------------------

            // 메인 이동 및 점프 로직
            // AI가 점프 필요하다고 판단했거나 (needJump),
            // 또는 갇힌 상태일 때 (stuck) - 플레이어 Y 위치와 관계없이 벽을 넘기 위한 점프
            // 그리고 JumpMove가 존재하며 물리적으로 점프할 준비가 되어있을 때
            if (jumper != null && (needJump || stuck) && jumper.ReadyToPerformJump())
            {
                float boostY = wallAhead ? statData.jumpForce * 0.2f : 0f;
                // toPl.y의 절대값을 사용하여 점프 높이 조절
                if (jumper.DoJump(dir, Mathf.Abs(toPl.y), statData.jumpForce + boostY, statData.jumpHorizontalFactor, jumpCd))
                {
                    Debug.Log($"[ChaseState] 점프 시도됨! 조건: (벽 앞: {wallAhead}, 갇힘: {stuck}), 목표 Y: {toPl.y}");
                }
                jumpTimer = jumpCd;
            }
            // 점프하지 않는 일반적인 수평 이동 상황
            else if (jumper == null || !jumper.IsMidJump)
            {
                float spiderBoost = (IsSpider && dist < statData.chargeRange) ? 2f : 1f;
                float targetX = dir * statData.moveSpeed * spiderBoost;

                if (grounded)
                {
                    if (edgeAhead && appliesEdgeStop)
                    {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                    }
                    else if (wallAhead && grounded) // 벽에 막혔을 때 속도 강제 0
                    {
                        rb.velocity = new Vector2(0f, rb.velocity.y);
                    }
                    else // 일반 이동
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
            if (climber != null && Mathf.Abs(toPl.y) > climber.MinYThreshold)
                climber.ScanAheadAndClimb(dir);

            /* -------- 이동 중 사격 (Range Attack Monster는 여기서 사격하지 않습니다. AttackState에서 처리) -------- */
            // 이 로직은 Range Attack Monster가 AttackState로 진입 후,
            // attackRange를 벗어났지만 chaseRange 안에 있을 때 다시 사격해야 하는 경우가 아니라면 제거해야 합니다.
            // 현재 요청사항에 맞춰, 이 부분은 주석 처리하거나 삭제하는 것이 좋습니다.
            /*
            if (isRanged && dist > statData.attackRange && dist <= statData.chaseRange && Time.time - lastShot >= statData.rangedCooldown)
            {
                Shoot(); 
                lastShot = Time.time;
            }
            */
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

        // Shoot() 메서드는 이제 AttackState에서 호출되도록 의도되거나,
        // MonsterBrain 내에 공통적으로 존재하도록 하는 것이 좋습니다.
        // ChaseState에서 더 이상 직접 호출하지 않습니다.
        void Shoot()
        {
            if (muzzle == null) return;
            Vector2 playerToMuzzleDir = (sPlayer.position - muzzle.position).normalized;

            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null) { Debug.LogError("ProjectilePool is null"); return; }

            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, playerToMuzzleDir);
                p.Launch(playerToMuzzleDir, statData.projectileSpeed);
            }
            else
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = pellets > 1 ? spread / (pellets - 1) : 0f;

                for (int i = 0; i < pellets; ++i)
                {
                    float angle = -spread * 0.5f + step * i;
                    Vector2 fireDir = Quaternion.AngleAxis(angle, Vector3.forward) * playerToMuzzleDir;

                    Projectile p = pool.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.FromToRotation(Vector2.right, fireDir);
                    p.Launch(fireDir, statData.projectileSpeed);
                }
            }
        }
        #endregion
    }
}