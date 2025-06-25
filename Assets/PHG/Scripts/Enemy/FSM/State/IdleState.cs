
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
namespace PHG
{
    // 몬스터의 Idle 상태
    // PatrolRange 이내에 플레이어가 감지되면 Chase 상태로 전환
    // FlyingTag가 있으면 수직으로 떠다니는 동작을 수행
    // PatrolRange 이내에 플레이어가 없으면 Idle 상태 유지
    public class IdleState : IState
    {
        readonly Rigidbody2D rb;
        private MonsterStats stats;
        private MonsterBrain brain;
        private static Transform sPlayer;
        public IdleState(MonsterBrain brain)
        {
            rb = brain.GetComponent<Rigidbody2D>();
            this.brain = brain;
            this.stats = brain.Stats;
        }

        public void Enter()
        {
            rb.velocity = Vector2.zero;
        }

        public void Tick()
        {
            if (stats.UsePatrol)
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
            if (dist < stats.PatrolRange)
            {
                brain.ChangeState(StateID.Chase); // ← FloatChaseState 진입
                Debug.Log($"{brain.name} detected player → entering FloatChase");
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }
    }
}
