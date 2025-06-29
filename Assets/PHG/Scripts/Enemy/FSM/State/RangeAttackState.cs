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
        readonly MonsterStatData statData;
        readonly bool isFlying;

        Transform player;
        Transform muzzle;
        float lastShot;

        /* ───────── cached values ───────── */
        float AttackR => brain.Stats.AttackRange;
        float ReadyR => statData.readyRange;
        float ChaseR => brain.Stats.ChaseRange;
        float Cooldown => statData.rangedCooldown;
        float MoveSpd => statData.moveSpeed;
        float AirAccel => 8f;

        /* -------------------------------------------------- */
        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            statData = brain.StatData;
            muzzle = tf.Find("MuzzlePoint");
            isFlying = brain.GetComponent<FlyingTag>() != null;
        }

        /* ================= IState ========================= */
        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
            lastShot = Time.time - Cooldown; // 첫 프레임 즉시 사격 허용
            FacePlayer();
        }

        public void Tick()
        {
            if (player == null) { brain.ChangeState(StateID.Patrol); return; }

            float dist = Vector2.Distance(tf.position, player.position);

            /* 1) 추격 포기 */

            if (dist > ChaseR)
            { if(isFlying) brain.ChangeState(StateID.Idle);
                else
               brain.ChangeState(StateID.Patrol); return; 
            }

            /* 2) Ready 범위 밖이면 ChaseState */
            if (dist > ReadyR) { brain.ChangeState(StateID.Chase); return; }

            /* 3) Ready 안 – Aim & 이동/사격 */
            FacePlayer();

            bool grounded = isFlying
                           ? false
                           : Physics2D.Raycast(tf.position, Vector2.down, 0.05f, brain.groundMask);
            int dir = (player.position.x >= tf.position.x) ? 1 : -1;

            if (dist > AttackR)   /* ─── 추격 구간 ─── */
            {
                float speed = MoveSpd;

                if (isFlying)     // 비행형: XY 모두 추적
                    rb.velocity = (player.position - tf.position).normalized * speed;
                else             // 지상형: 기존 로직
                {
                    float targetX = dir * speed;
                    if (grounded)
                        rb.velocity = new Vector2(targetX, rb.velocity.y);
                    else
                        rb.velocity = new Vector2(
                            Mathf.MoveTowards(rb.velocity.x, targetX, AirAccel * Time.deltaTime),
                            rb.velocity.y);
                }
            }
            else                 /* ─── 사격 구간 ─── */
            {
                rb.velocity = Vector2.zero;
                if (Time.time - lastShot >= Cooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        /* ---------------- helpers ------------------------- */
        void Shoot()
        {
            if (muzzle == null || player == null) return;
            var pool = ProjectilePool.Instance;
            if (pool == null) { Debug.Log("[RangeAttackState] Pool is null"); return; }

            Vector2 baseDir = (player.position - muzzle.position).normalized;
            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatData.FirePattern.Single)
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
                p.Launch(baseDir);
            }
            else
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
                    p.Launch(dir);
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