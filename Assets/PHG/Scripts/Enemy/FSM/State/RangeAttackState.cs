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
        readonly MonsterStatEntry statData; // MonsterStatData -> MonsterStatEntry로 변경
        readonly bool isFlying;

        Transform player;
        Transform muzzle;
        float lastShot;

        /* ───────── cached values ───────── */
        // brain.Stats -> statData 직접 참조로 변경
        float AttackR => statData.attackRange;
        float ReadyR => statData.readyRange;
        float ChaseR => statData.chaseRange;
        float Cooldown => statData.rangedCooldown;
        float MoveSpd => statData.moveSpeed;
        float AirAccel => 8f; // 이 값은 statData에 없으므로 그대로 둡니다.

        /* -------------------------------------------------- */
        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb; // brain.GetComponent<Rigidbody2D>() -> brain.rb로 변경
            tf = brain.tf; // brain.transform -> brain.tf로 변경
            statData = brain.statData; // brain.StatData -> brain.statData로 변경 (프로퍼티 이름 변경)
            muzzle = tf.Find("MuzzlePoint");
            isFlying = brain.GetComponent<FlyingTag>() != null;
        }

        /* ================= IState ========================= */
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
                if (player == null) return; // 플레이어를 찾지 못하면 아무것도 하지 않음
            }

            // ★★★ 수정: Vector3를 Vector2로 명시적 형변환 ★★★
            Vector2 toPl = (Vector2)player.position - (Vector2)tf.position;
            float dist = toPl.magnitude;

            FacePlayer();

            // 추격 범위 내에 없으면 추격 상태로 전환
            if (dist > statData.chaseRange)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            // 공격 범위 내에 있으면 공격
            if (dist <= statData.attackRange)
            {
                rb.velocity = Vector2.zero; // 공격 중에는 정지
                if (Time.time - lastShot >= statData.rangedCooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }
                return; // 공격 로직 처리 후 이동 로직은 스킵
            }

            // 사격 대기 범위 내에서 플레이어와 거리를 유지하며 이동
            if (dist <= statData.readyRange)
            {
                // 플레이어와 가까워지면 뒤로, 멀어지면 앞으로 이동 (원거리 몬스터 특유의 거리 유지)
                Vector2 targetVel = -toPl.normalized * MoveSpd; // 거리를 좁히거나 벌릴 때 사용

                if (isFlying) // 비행 몬스터
                {
                    rb.velocity = Vector2.Lerp(rb.velocity, targetVel, Time.deltaTime * AirAccel);
                }
                else // 일반 몬스터 (지상)
                {
                    // 벽 감지 (벽에 막히면 움직임을 멈춤)
                    RaycastHit2D wallCheck = Physics2D.Raycast(tf.position, tf.right * Mathf.Sign(tf.localScale.x), statData.attackRange, brain.groundMask);
                    if (wallCheck.collider != null)
                    {
                        rb.velocity = Vector2.zero; // 벽에 막히면 정지
                    }
                    else
                    {
                        rb.velocity = new Vector2(targetVel.x, rb.velocity.y);
                    }
                }
            }
            else // 사격 대기 범위를 벗어나면 추격 (필요시)
            {
                // 현재 RangeAttackState에 진입했다는 것은 이미 추격 범위 내라는 가정.
                // readyRange를 벗어나면 다시 chaseRange까지 추격하거나, 단순 이동 로직을 추가할 수 있습니다.
                Vector2 targetVel = toPl.normalized * MoveSpd;
                if (isFlying)
                {
                    rb.velocity = Vector2.Lerp(rb.velocity, targetVel, Time.deltaTime * AirAccel);
                }
                else
                {
                    rb.velocity = new Vector2(targetVel.x, rb.velocity.y);
                }
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }

        /* -------------------------------------------------- */
        void Shoot()
        {
            // ProjectilePool 인스턴스가 있는지 확인
            ProjectilePool pool = ProjectilePool.Instance;
            if (pool == null) { Debug.LogError("ProjectilePool is null"); return; }

            Vector2 baseDir = (player.position - muzzle.position).normalized;
            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatEntry.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
                p.Launch(baseDir, statData.projectileSpeed);
            }
            else // Spread 패턴
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = pellets > 1 ? spread / (pellets - 1) : 0f;

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
            int sign = (player.position.x >= tf.position.x) ? 1 : -1;
            Vector3 sc = tf.localScale;
            sc.x = Mathf.Abs(sc.x) * sign;
            tf.localScale = sc;
        }
    }
}