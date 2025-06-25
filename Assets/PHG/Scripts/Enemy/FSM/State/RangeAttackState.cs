using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PHG
{
    public class RangeAttackState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;

        // turnables
        private const float attackCooldown = 1.5f;

        //runtime
        private Transform player;
        private float lastShot;


        public RangeAttackState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>();
            tf = brain.transform;
        }

        public void Enter()
        {
            brain.ChangeState(StateID.Chase); // 즉시 추격으로 복귀
            player = GameObject.FindWithTag("Player")?.transform;
            rb.velocity = Vector2.zero;
            lastShot = -attackCooldown;
        }

        public void Tick()
        {
          if(player == null)
            {
                brain.ChangeState(StateID.Patrol);
                return;
            }
            float dist = Vector2.Distance(tf.position, player.position);
        if(dist > brain.Stats.AttackRange)
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            int dir = player.position.x > tf.position.x ? 1 : -1;
            tf.localScale = new Vector3(dir, 1f, 1f);

            rb.velocity = Vector2.zero;
            if(Time.time - lastShot >= attackCooldown)
            {
                shoot(dir);
                lastShot = Time.time;
            }
        
        }

        private void shoot(int dir)
        {
            Projectile proj = ProjectilePool.Instance.Get();
            proj.transform.position = tf.position + Vector3.right * dir * 0.6f;
            proj.Launch(Vector2.right * dir);
            //애니메이션 및 사운드;
        }
        public void Exit() => rb.velocity = Vector2.zero;
    }

}