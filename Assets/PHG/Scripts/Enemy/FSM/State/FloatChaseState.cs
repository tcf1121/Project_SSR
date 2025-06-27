using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;

namespace PHG
{
    public class FloatChaseState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private static Transform sPlayer;
        private readonly MonsterStatData statData;
/*원거리 공격용*/
        private readonly bool isRanged;
        Transform muzzle;
        float lastShot;
        public FloatChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
            statData = brain.StatData;
            if(brain.GetComponent<RangedTag>() != null)
            {
                isRanged = brain.GetComponent<RangedTag>() != null;
                muzzle = tf.Find("MuzzlePoint");
            }
            else
            {
                isRanged = false;
            }
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
                float speed = brain.Stats.MoveSpeed * (dist < brain.Stats.ChargeRange ? statData.rushMultiplier : 1f);


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
            /*원거리 공격용*/
            if (isRanged)
            {
                const float floatSpeed = 1.5f;
                const float floatAmplitude = 0.2f;
                float bobY = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

                /* 0) 추적 범위 이탈 → 정지 + Idle 복귀 */
                if (dist > brain.Stats.ChaseRange)
                {
                    rb.velocity = new Vector2(0f, bobY);
                    brain.ChangeState(StateID.Idle);
                    return;
                }

                /* 1) readyRange 안 → 정지 + AttackState 전환 */
                if (dist <= statData.readyRange)
                {
                    rb.velocity = new Vector2(0f, bobY);
                    brain.ChangeState(StateID.Attack);   // RangeAttackState
                    return;
                }

                /* 2) 이동 사격 구간 (attackRange ~ readyRange) */
                if (dist > statData.attackRange &&
                    Time.time - lastShot >= statData.rangedCooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }

                /* 3) 추적 이동하면서 항상 둥둥 */
                 if (dist > statData.readyRange)
                   rb.velocity = new Vector2(0f, bobY);      // ← 멈춤
                 else
                   rb.velocity = new Vector2(rb.velocity.x, bobY);  // 이동 사격 구간은 X 유지
            }
        }

        public void Exit()
        {
            rb.velocity = Vector2.zero;
        }

        /*원거리 공격용*/
        void Shoot()
        {
            if (muzzle == null) return;
            Vector2 dir = (sPlayer.position - muzzle.position).normalized;
            Projectile proj = ProjectilePool.Instance.Get(statData.projectileprefab, muzzle.position);
            proj.Launch(dir);
        }
    }
}
