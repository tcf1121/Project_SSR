using JetBrains.Annotations;
using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 원거리 몬스터 – 공격/조준/추격 제어
    /// </summary>
    public class RangeAttackState : IState
    {
        /* ───────── refs ───────── */
        readonly MonsterBrain brain;          // (멤버 변수)
        readonly Rigidbody2D rb;             // (멤버 변수)
        readonly Transform tf;             // (멤버 변수)
        readonly MonsterStatData statData;       // (멤버 변수)

        Transform player;                        // (멤버 변수)
        Transform muzzle;                        // (멤버 변수)
        float lastShot;                      // (멤버 변수)

        /* ───────── cached values ───────── */
        float AttackR => brain.Stats.AttackRange;   // 근거리 사격 범위
        float ReadyR => statData.readyRange;        // 조준 유지 범위
        float ChaseR => brain.Stats.ChaseRange;     // 추격 중단 범위
        float Cooldown => statData.rangedCooldown;    // 발사 간격
        float MoveSpd => statData.moveSpeed;         // 이동 속도 (grounded 기준)
        float AirAccel => 8f;                         // 공중 가속 (고정)

        /* -------------------------------------------------- */
        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            statData = brain.StatData;
            muzzle = tf.Find("MuzzlePoint");
        }

        /* ================= IState ========================= */
        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
            lastShot = Time.time - Cooldown;   // 처음 바로 쏠 수 있도록 초기화
            FacePlayer();
        }

        public void Tick()
        {
            if (player == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            float dist = Vector2.Distance(tf.position, player.position);

            /* 1) 추격 포기 */
            if (dist > ChaseR)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* 2) readyRange 밖이면 ChaseState로 이동 */
            if (dist > ReadyR)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            /* 3) readyRange 안 – Aim & (조건부) 이동/사격 */
            FacePlayer();

            bool grounded = Physics2D.Raycast(tf.position, Vector2.down, 0.05f, brain.groundMask);
            int dir = (player.position.x >= tf.position.x) ? 1 : -1;

            if (dist > AttackR)   // 공격 범위를 벗어남 → 이동하며 재접근
            {
                float targetX = dir * MoveSpd;
                if (grounded)
                    rb.velocity = new Vector2(targetX, rb.velocity.y);
                else
                    rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetX, AirAccel * Time.deltaTime), rb.velocity.y);
            }
            else                  // AttackR 안 → 정지 후 사격
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
                if (Time.time - lastShot >= Cooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }
            }

            if (brain.GetComponent<FlyingTag>() != null)
            {
                if (player == null) return;
                int sign = (player.position.x >= tf.position.x) ? 1 : -1;
                Vector3 sc = tf.localScale;
                sc.x = -Mathf.Abs(sc.x) * sign;
                tf.localScale = sc;
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        /* ---------------- helpers ------------------------- */
        void Shoot()
        {
            if (muzzle == null || player == null) return;
            var pool = ProjectilePool.Instance;

            if (pool == null)
            { Debug.Log("[RangeAttatckState] 풀 is Null"); return; }
            Vector2 baseDir = (player.position - muzzle.position).normalized;
            Projectile prefab = statData.projectileprefab;

            if (statData.firePattern == MonsterStatData.FirePattern.Single) //단발
            {
                Projectile p = pool.Get(prefab, muzzle.position);
                p.transform.rotation = Quaternion.FromToRotation(Vector2.right, baseDir);
                p.Launch(baseDir);
            }
            else
            {
                int pellets = Mathf.Max(1, statData.pelletCount);
                float spread = statData.spreadAngle;
                float step = pellets > 1 ? spread / (pellets - 1) : 0f; //간격

                for (int i = 0; i < pellets; i++)
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