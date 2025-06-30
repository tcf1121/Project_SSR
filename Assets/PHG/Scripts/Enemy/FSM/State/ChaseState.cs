using UnityEngine;

namespace PHG
{
    public class ChaseState : IState
    {
        private static Transform sPlayer;
        public static Transform Player => sPlayer;

        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly MonsterStatEntry statData;
        private readonly IMonsterJumper jumper;
        private readonly Transform wallSensor;

        private readonly bool isRanged;
        private readonly Transform muzzle;

        private const float WALL_CHECK_DIST = 0.25f;
        private const float STUCK_VEL_TOL = 0.05f;

        private float lastShot;

        public ChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb;
            tf = brain.tf;
            statData = brain.statData;
            wallSensor = brain.wallSensor;
            jumper = brain;                // IMonsterJumper 구현체
            isRanged = brain.IsRanged;
            if (isRanged) muzzle = tf.Find("MuzzlePoint");
        }

        /* ───────── IState ───────── */
        public void Enter()
        {
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

            lastShot = Time.time;
        }

        public void Tick()
        {
            if (sPlayer == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* --- 기본 벡터 및 판정 --- */
            Vector2 toPl = (Vector2)sPlayer.position - (Vector2)tf.position;
            float dist = toPl.magnitude;
            int dir = (toPl.x > 0) ? 1 : -1;

            bool grounded = brain.CanJump ? jumper.IsGrounded()
                                          : CheckGroundedFallback();
            bool midJump = brain.CanJump ? jumper.IsMidJump : false;

            if (!midJump && grounded)
                Orient(dir);

            /* --- 벽/점프 처리 (점프 가능 개체만) --- */
            Vector2 rayDir = tf.right * Mathf.Sign(tf.localScale.x);
            RaycastHit2D wallCheck = Physics2D.Raycast(
                wallSensor.position, rayDir, WALL_CHECK_DIST,
                LayerMask.GetMask("Ground", "Platform"));

            bool wallAhead = wallCheck.collider != null;
            bool stuck = grounded && Mathf.Abs(rb.velocity.x) < STUCK_VEL_TOL;
            bool targetAbove = toPl.y > 0.5f;

            if (brain.CanJump &&
                grounded && !midJump && jumper.ReadyToJump() &&
                wallAhead && stuck && targetAbove)
            {
                float dy = Mathf.Abs(toPl.y);
                jumper.PerformJump(dir, dy,
                                   statData.jumpForce,
                                   statData.jumpHorizontalFactor,
                                   statData.jumpCooldown);
                return;
            }

            /* --- 수평 이동 --- */
            if (grounded && !midJump)
            {
                float chargeBoost =
                    (!isRanged && brain.IsCharging && dist < statData.chargeRange)
                    ? statData.rushMultiplier
                    : 1f;

                float targetX = dir * statData.moveSpeed * chargeBoost;
                rb.velocity = new Vector2(targetX, rb.velocity.y);
            }

            /* 공중 유지속도(점프 중) */
            if (!grounded && midJump)
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);

            /* 추적 종료 판정 */
            if (dist > statData.chaseRange)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* 사다리 AI */
            if (brain.CanClimbLadders && Mathf.Abs(toPl.y) > 1.25f)
                brain.Climber?.TryFindAndClimb(dir, sPlayer.position);

            /* 원거리형 발사 로직 */
            if (isRanged)
            {
                if (dist <= statData.attackRange)
                {
                    brain.ChangeState(StateID.Attack);
                    return;
                }

                if (dist > statData.readyRange && dist <= statData.chaseRange)
                {
                    if (Time.time - lastShot >= statData.rangedCooldown)
                    {
                        Orient(dir);
                        Shoot();
                        lastShot = Time.time;
                        rb.velocity = new Vector2(0f, rb.velocity.y);
                        return;
                    }
                }
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        /* ───────── Helper ───────── */
        private void Orient(int dir)
        {
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            tf.localScale = s;
        }

        private bool CheckGroundedFallback()
        {
            // sensor 기준 간단한 레이캐스트 – JumpMove 미사용 시 전용
            return Physics2D.Raycast(
                       brain.sensor.position,
                       Vector2.down,
                       0.15f,
                       statData.groundMask);
        }

        private void Shoot()
        {
            if (muzzle == null) return;
            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null) return;

            Vector2 baseDir = (sPlayer.position - muzzle.position).normalized;
            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
                p.Launch(baseDir, statData.projectileSpeed);
            }
            else
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = (pellets > 1) ? spread / (pellets - 1) : 0f;

                for (int i = 0; i < pellets; ++i)
                {
                    float angle = -spread * 0.5f + step * i;
                    Vector2 dirVec = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;

                    Projectile p = pool.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.FromToRotation(Vector2.right, dirVec);
                    p.Launch(dirVec, statData.projectileSpeed);
                }
            }
        }
    }
}