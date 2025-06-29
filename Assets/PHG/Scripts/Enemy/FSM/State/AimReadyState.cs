using UnityEngine;

namespace PHG
{
    /// <summary>
    /// 플레이어가 10타일 내에 있으면 정지‧조준만 하고
    /// AttackRange 안으로 들어오면 RangeAttackState로 넘긴다.
    /// </summary>
    public class AimReadyState : IState
    {
        readonly MonsterBrain brain;
        readonly Rigidbody2D rb;
        readonly Transform tf;
        readonly MonsterStatEntry stat;
        Transform player;

        const float READY_RANGE = 10f;   // 요구사항: 10타일
        float sqReady, sqAttack;         // 성능용 제곱거리

        public AimReadyState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            stat = brain.statData;

            sqReady = READY_RANGE * READY_RANGE;
            sqAttack = brain.statData.attackRange * brain.statData.attackRange;
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

            /* --- 거리 판정 -------------------------------------------------- */
            float sqDist = (player.position - tf.position).sqrMagnitude;

            if (sqDist > sqReady)                 // 10타일 밖 → 복귀
            {
                brain.ChangeState(StateID.Patrol); // 필요하면 Chase로
                return;
            }
            if (sqDist <= sqAttack)               // 사정거리 진입
            {
                brain.ChangeState(StateID.Attack);
                return;
            }

            /* --- 정지 & 방향만 갱신 ---------------------------------------- */
            rb.velocity = Vector2.zero;
            int dir = player.position.x > tf.position.x ? 1 : -1;
            tf.localScale = new Vector3(Mathf.Abs(tf.localScale.x) * dir, 1, 1);
        }

        public void Exit() => rb.velocity = Vector2.zero;
    }
}