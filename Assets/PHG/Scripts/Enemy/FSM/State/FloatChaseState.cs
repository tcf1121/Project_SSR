using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    public class FloatChaseState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private static Transform sPlayer;
        private readonly MonsterStatData statData;

        public FloatChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            statData = brain.StatData;
        }

        public void Enter()
        {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

        }

        public void Tick()
        {
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;

            if (sPlayer == null)
                return; // 여긴 진짜 못 찾았을 때만 나감

            Vector2 currentPos = tf.position;
            Vector2 targetPos = sPlayer.position;
            Vector2 dir = (targetPos - currentPos).normalized;
            float dist = Vector2.Distance(currentPos, targetPos);


            if (dist <= brain.Stats.ChaseRange)
            {
                float speed = dist < brain.Stats.ChargeRange
                              ? brain.Stats.MoveSpeed
                              : brain.Stats.MoveSpeed * 0.5f;


                rb.velocity = dir * speed;


                if (Mathf.Abs(dir.x) > 0.05f)
                {
                    Vector3 scale = tf.localScale;
                    scale.x = -Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
                    tf.localScale = scale;
                }

            }
            else
            {
                float floatSpeed = 1.5f;
                float floatAmplitude = 0.2f;
                rb.velocity = new Vector2(0f, Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }
    }
}
