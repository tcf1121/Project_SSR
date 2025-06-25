using UnityEngine;

namespace PHG
{
    /// <summary>
    /// State – 근접 공격
    /// 플레이어가 사정거리 안에 들어오면 정지 후 타격
    /// </summary>
    public class MeleeAttackState : IState
    {
        /* ───── refs ───── */
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly Collider2D hitBox;  // (멤버 변수) 피격 판정용

        /* ───── tunables ───── */
        private const float swingCooldown = 0.6f;          // (멤버 변수) 공격 간격

        /* ───── runtime ───── */
        private Transform player;
        private float lastSwing;

        public MeleeAttackState(MonsterBrain brain, Collider2D hitBox)
        {
            this.brain = brain;
            this.hitBox = hitBox;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
        }

        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            hitBox.enabled = false;
            rb.velocity = Vector2.zero;
            lastSwing = -swingCooldown;
        }

        public void Tick()
        {
            if (player == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            float dist = Vector2.Distance(tf.position, player.position);

            /* 사정거리 밖 → 추격 */
            if (dist > brain.Stats.AttackRange)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            /* 방향 고정 */
            int dir = player.position.x > tf.position.x ? 1 : -1;
            tf.localScale = new Vector3(dir, 1, 1);

            /* 공격 */
            if (Time.time - lastSwing >= swingCooldown)
            {
                rb.velocity = Vector2.zero;
               // brain.Animator.SetTrigger("Swing");   // 애니메이션 트리거 (선택)
                lastSwing = Time.time;
            }
        }

        /// <summary>
        /// Animation Event로 호출 – 실제 판정 구간만 활성화
        /// </summary>
        public void ActivateHitBox() => hitBox.enabled = true;
        public void DeactivateHitBox() => hitBox.enabled = false;

        public void Exit()
        {
            hitBox.enabled = false;
            rb.velocity = Vector2.zero;
        }
    }
}