using UnityEngine;

namespace PHG
{
    public class FloatChaseState : IState
    {
        /* ───── refs ───── */
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private static Transform sPlayer;        // 플레이어 캐싱
        private readonly MonsterStatEntry statData;

        /* 원거리 전용 */
        private readonly bool isRanged;
        private readonly Transform muzzle;

        public FloatChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb;
            tf = brain.tf;
            statData = brain.StatData;

            /* Tag 의존 제거 —> Ability Flag 사용 */
            isRanged = brain.IsRanged;
            if (isRanged)
                muzzle = tf.Find("MuzzlePoint");   // 원거리 전용 총구
        }

        /* -------------------------------------------------------------- */
        #region IState
        public void Enter()
        {
            rb.gravityScale = 0f;      // 비행: 중력 제거
            rb.velocity = Vector2.zero;

            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Walk);
        }

        public void Tick()
        {
            if (sPlayer == null)
            {
                sPlayer = GameObject.FindWithTag("Player")?.transform;
                if (sPlayer == null) return;        // 플레이어 못 찾음
            }

            Vector2 currentPos = tf.position;
            Vector2 toPl = (Vector2)sPlayer.position - currentPos;
            float dist = toPl.magnitude;

            /* 추격 범위 벗어나면 Idle */
            if (dist > statData.chaseRange * 1.5f)
            {
                brain.ChangeState(StateID.Idle);
                return;
            }

            /* ───── 원거리형 로직 ───── */
            if (isRanged)
            {
                /* 사정거리 내 → Shoot (RangedAttackBehavior 사용) */
                if (dist <= statData.attackRange) // Cooldown is handled by RangedAttackBehavior
                {
                    // RangedAttackBehavior를 통해 발사 로직 실행
                    brain.attackBehavior.Execute(brain);
                }

                /* readyRange 바깥이면 추격, 안이면 멈춤 */
                if (dist > statData.readyRange)
                {
                    Vector2 dir = toPl.normalized;
                    rb.velocity = dir * statData.moveSpeed;
                    Orient(dir);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
                return; // 원거리형은 여기서 끝
            }

            /* ───── 근접/일반 추격 로직 ───── */
            if (dist <= statData.chaseRange)
            {
                Vector2 dir = toPl.normalized;
                float speed = statData.moveSpeed *
                                (dist < statData.chargeRange ? statData.rushMultiplier : 1f);
                rb.velocity = dir * speed;
                Orient(dir);
            }
            else
            {
                /* 부유 애니메이션만 */
                float bobY = Mathf.Sin(Time.time * 1.5f) * 0.2f;
                rb.velocity = new Vector2(0f, bobY);
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;
        #endregion

        /* -------------------------------------------------------------- */
        #region helpers
        // Shoot() 메서드는 RangedAttackBehavior.Execute()로 대체되어 제거되었습니다.
        void Orient(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > 0.05f)
            {
                Vector3 sc = tf.localScale;
                sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x);
                tf.localScale = sc;
            }
        }
        #endregion
    }
}