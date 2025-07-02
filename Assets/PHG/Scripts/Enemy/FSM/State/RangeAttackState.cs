using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 원거리 몬스터 – 공격/조준/추격 제어
    /// </summary>
    public class RangeAttackState : IState
    {
        /* ───────── refs ───────── */
        readonly MonsterBrain brain;
        readonly Rigidbody2D rb;
        readonly Transform tf;
        readonly MonsterStatEntry statData;
        readonly bool isFlying;

        Transform player;
        Transform muzzle;
        float lastShot;

        /* ───────── cached values ───────── */
        float AttackR => statData.attackRange;
        float ReadyR => statData.readyRange;
        float ChaseR => statData.chaseRange;
        float Cooldown => statData.rangedCooldown;
        float MoveSpd => statData.moveSpeed;
        const float AirAccel = 8f;

        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb;
            tf = brain.tf;
            statData = brain.StatData;
            muzzle = tf.Find("MuzzlePoint");
            isFlying = brain.IsFlying;
        }

        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
            lastShot = Time.time;
        }

        public void Tick()
        {
            if (player == null)
            {
                player = GameObject.FindWithTag("Player")?.transform;
                if (player == null) return;
            }

            Vector2 toPl = (Vector2)player.position - (Vector2)tf.position;
            float dist = toPl.magnitude;

            FacePlayer();

            /* --- 상태 전이 처리 --- */

            if (dist > ChaseR)
            {
                brain.ChangeState(StateID.Chase); // 추격 포기
                return;
            }

            if (dist > ReadyR && dist <= ChaseR)
            {
                brain.ChangeState(StateID.Chase); // 다시 추격 상태로
                return;
            }

            if (dist <= AttackR)
            {
                rb.velocity = Vector2.zero;

                if (Time.time - lastShot >= Cooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }

                return; // 사격 후 이동 생략
            }

            /* --- readyRange 내부일 때 거리 유지 (뒤로 물러남) --- */

            Vector2 targetVel = -toPl.normalized * MoveSpd;

            if (isFlying)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, targetVel, Time.deltaTime * AirAccel);
            }
            else
            {
                rb.velocity = new Vector2(targetVel.x, rb.velocity.y);
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        void Shoot()
        {
            if (muzzle == null) return;
            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null)
            {
                Debug.LogError("ProjectilePool is null");
                return;
            }

            Vector2 baseDir = (player.position - muzzle.position).normalized;
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
                    Vector2 dir = Quaternion.AngleAxis(angle, Vector3.forward) * baseDir;

                    Projectile p = pool.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.FromToRotation(Vector2.right, dir);
                    p.Launch(dir, statData.projectileSpeed);
                }
            }
        }

        void FacePlayer()
        {
            if (player == null) return;
            int sign = player.position.x >= tf.position.x ? 1 : -1;
            Vector3 sc = tf.localScale;
            sc.x = Mathf.Abs(sc.x) * sign;
            tf.localScale = sc;
        }
    }
}