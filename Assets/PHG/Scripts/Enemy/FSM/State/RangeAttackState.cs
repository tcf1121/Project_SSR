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
        public Transform muzzle;
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
            muzzle = brain.Muzzle;
            isFlying = brain.IsFlying;
        }

        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;

            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Attack);


            lastShot = Time.time;
        }

        public void Tick()
        {
            // 1. 쿨타임 확인
            if (Time.time - lastShot < Cooldown)
                return;

            // 2. 플레이어 유효성 및 거리 체크
            if (player == null) return;
            float dist = Vector2.Distance(tf.position, player.position);

            if (dist > ChaseR)
            {
               // brain.ChangeState(StateID.Chase);
                return;
            }
           
            if (dist > AttackR)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            FacePlayer();
            // 3. 공격 범위 안이면 정지 후 애니메이션만 재생
            if (brain.IsGrounded() && !brain.IsMidJump)
                rb.velocity = Vector2.zero;

            // Execute() 직접 호출 삭제!
            brain.animator.Play(AnimNames.Attack, 0, 0f);

            // 4. 쿨타임 초기화
            lastShot = Time.time;
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

                p.Launch(baseDir, statData.projectileSpeed, brain);
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
                    p.Launch(dir, statData.projectileSpeed, brain);
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