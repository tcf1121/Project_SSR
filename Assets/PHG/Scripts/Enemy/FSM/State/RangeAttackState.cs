using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 공격/추적 규칙
    /// 1) d > chaseRange                 → Patrol
    /// 2) chaseRange ≥ d > readyRange    → Chase
    /// 3) readyRange ≥ d > attackRange   → Aim (정지)
    ///    단, hasFired == true           → Chase (계속 추격)
    /// 4) attackRange ≥ d                → Shoot
    /// </summary>
    public class RangeAttackState : IState
    {
        readonly MonsterBrain brain;
        readonly Rigidbody2D rb;
        readonly Transform tf;
        readonly MonsterStatData statData;

        Transform player;
        Transform muzzle;
        float lastShot;
        bool hasFired;                     // ★ 발사 여부 플래그

        float Cooldown => statData.rangedCooldown;

        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            statData = brain.StatData;

            muzzle = tf.Find("MuzzlePoint");
            if (muzzle == null)
                Debug.LogWarning($"{tf.name} 에서 MuzzlePoint를 찾을 수 없습니다.");
        }

        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
            lastShot = -Cooldown;
            hasFired = false;              // ★ 초기화
        }

        public void Tick()
        {
            if (player == null) { brain.ChangeState(StateID.Patrol); return; }

            float dist = Mathf.Abs(player.position.x - tf.position.x);
            float attackR = brain.Stats.AttackRange;
            float readyR = statData.readyRange;
            float chaseR = brain.Stats.ChaseRange;

            /* 1) 추격 포기 */
            if (dist > chaseR) { brain.ChangeState(StateID.Patrol); return; }

            /* 2) 추격 유지 */
            if (dist > readyR) { brain.ChangeState(StateID.Chase); return; }

            /* 3 & 4) readyRange 안 */
            rb.velocity = Vector2.zero;
            FacePlayer();

            if (dist <= attackR)                               // 4) 발사 구간
            {
                if (Time.time - lastShot >= Cooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }
            }
            else                                               // 3) Aim 구간
            {
                if (hasFired)                                  // 이미 공격 중이면 추격
                {
                    brain.ChangeState(StateID.Chase);
                }
                // hasFired == false → 첫 진입 : 그대로 서 있기
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        /* ───── helpers ───── */
        void Shoot()
        {
            if (muzzle == null || player == null) return;

            Vector2 dir = (player.position - muzzle.position).normalized;
            Projectile p = ProjectilePool.Instance.Get(statData.projectileprefab, muzzle.position);
            p.Launch(dir);
            hasFired = true;                 // ★ 발사 플래그 ON
        }

        void FacePlayer()
        {
            int sign = player.position.x > tf.position.x ? 1 : -1;
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * sign;
            tf.localScale = s;
        }
    }
}