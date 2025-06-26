


using UnityEngine;

namespace PHG
{
    public class IdleState : IState
    {
        private readonly Rigidbody2D rb;
        private readonly MonsterBrain brain;
        private readonly MonsterStatData statData;
        private static Transform sPlayer;

        public IdleState(MonsterBrain brain)
        {
            this.brain = brain;
            this.statData = brain.StatData;
            this.rb = brain.GetComponent<Rigidbody2D>();
        }

        public void Enter()
        {
            rb.velocity = Vector2.zero;
        }

        public void Tick()
        {
           
            if (statData == null) return; // ✅ null 방어

            if (brain.Stats != null && brain.Stats.UsePatrol)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }

            if (brain.GetComponent<FlyingTag>() != null)
            {
                float floatSpeed = 2.5f;
                float floatAmplitude = 0.3f;
                rb.velocity = new Vector2(0f, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;
            if (sPlayer == null) return;

            float dist = Vector2.Distance(sPlayer.position, brain.transform.position);
            if (dist < statData.patrolRange)
            {
                brain.ChangeState(StateID.Chase);
#if UNITY_EDITOR
                Debug.Log($"{brain.name} detected player → entering FloatChase");
#endif
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }
    }
}