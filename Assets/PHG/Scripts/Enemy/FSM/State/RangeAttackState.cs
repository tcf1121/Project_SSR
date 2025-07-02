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

            // 지상일 때만 정지 – 공중 점프 중이면 유지
            if (brain.IsGrounded() && !brain.IsMidJump)
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

            // 추적 범위 밖 → 추적 재개
            if (dist > ChaseR)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            // 추적 유지 범위 → Chase로 복귀 (사격 불가한 거리)
            if (dist > AttackR && dist <= ChaseR)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            // 사격
            if (dist <= AttackR)
            {
                // ★ grounded일 때만 멈추기
                bool grounded = brain.IsGrounded();  // jumper.IsGrounded() 내부 호출
                if (grounded&&!brain.IsMidJump)
                    rb.velocity = Vector2.zero;

                if (Time.time - lastShot >= Cooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }

                return;
            }
        }
        public void Exit()
        {
            if (brain.IsGrounded() && !brain.IsMidJump)
                rb.velocity = Vector2.zero;

        }
        void Shoot()
        {
            if (muzzle == null) return;
            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null)
            {
                Debug.LogError("ProjectilePool is null");
                return;
            }

            // 조준 위치 보정 (Y값 내려서 상체나 머리 쪽 조준)
            Vector2 aimTarget = player.position + Vector3.up * 0.25f;  // ← 보정값 필요 시 조절
            Vector2 baseDir = (aimTarget - (Vector2)muzzle.position).normalized;

            // 기본 투사체
            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);

                // 회전은 Z축 기준 2D 전용으로 적용
                float angle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                p.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                p.Launch(baseDir, statData.projectileSpeed);
            }
            else
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = (pellets > 1) ? spread / (pellets - 1) : 0f;

                for (int i = 0; i < pellets; ++i)
                {
                    float angleOffset = -spread * 0.5f + step * i;
                    float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
                    float shotAngle = baseAngle + angleOffset;

                    Vector2 dir = Quaternion.AngleAxis(shotAngle, Vector3.forward) * Vector2.right;

                    Projectile p = pool.Get(prefab, muzzle.position);
                    p.transform.rotation = Quaternion.AngleAxis(shotAngle, Vector3.forward);
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