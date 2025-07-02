using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ▷ 원거리 몬스터 전용 대기 상태
    /// 플레이어가 readyRange 안에 들어오면 정지‧조준만 하고,
    /// attackRange 안으로 들어오면 RangeAttackState 로 전환.
    /// </summary>
    public class AimReadyState : IState
    {
        readonly MonsterBrain brain;
        readonly Rigidbody2D rb;
        readonly Transform tf;
        readonly MonsterStatEntry stat;
        Transform player;

        float sqReady, sqAttack;   // 거리 비교용 제곱값

        public AimReadyState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            stat = brain.StatData;

            /* 계산 캐싱 */
            sqReady = stat.readyRange * stat.readyRange;
            sqAttack = stat.attackRange * stat.attackRange;
        }

        public void Enter()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
        }

        public void Tick()
        {
            if (player == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            float sqDist = (player.position - tf.position).sqrMagnitude;

            /* 10타일 밖 → Patrol 복귀 */
            if (sqDist > sqReady)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* 사정거리 진입 → Attack 전환 */
            if (sqDist <= sqAttack)
            {
                brain.ChangeState(StateID.Attack);
                return;
            }

            /* 정지‧방향 전환만 수행 */
            rb.velocity = Vector2.zero;
            int dir = player.position.x > tf.position.x ? 1 : -1;
            tf.localScale = new Vector3(Mathf.Abs(tf.localScale.x) * dir, 1, 1);
        }

        public void Exit() => rb.velocity = Vector2.zero;
    }
}