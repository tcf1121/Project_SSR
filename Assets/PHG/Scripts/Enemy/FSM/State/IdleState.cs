using UnityEngine;

namespace PHG
{
    public class IdleState : IState
    {
        private readonly Rigidbody2D rb;
        private readonly MonsterBrain brain;
        private readonly MonsterStatEntry statData;
        private static Transform sPlayer;      // 플레이어 캐싱

        public IdleState(MonsterBrain brain)
        {
            this.brain = brain;
            this.statData = brain.StatData;
            this.rb = brain.GetComponent<Rigidbody2D>();
        }

        public void Enter() => rb.velocity = Vector2.zero;

        public void Tick()
        {
            if (statData == null) return;             // 안전 방어

            /* ── 순찰 플래그가 켜져 있으면 즉시 Patrol ── */
            if (statData.usePatrol)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            /* ── 비행 유닛은 Idle 상태에서도 부유 애니메이션 ── */
            if (brain.IsFlying)
            {
                float floatSpeed = 2.5f;
                float floatAmplitude = 0.3f;
                rb.velocity = new Vector2(0f,
                               Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

            /* ── 플레이어 감지 → Chase / FloatChase 전환 ── */
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;
            if (sPlayer == null) return;

            float dist = Vector2.Distance(sPlayer.position, brain.transform.position);
            if (dist < statData.patrolRange)
            {
                brain.ChangeState(StateID.Chase);     // 비행 유닛이면 내부에서 FloatChase로 매핑
#if UNITY_EDITOR
                Debug.Log($"{brain.name} detected player → enter Chase");
#endif
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;
    }
}