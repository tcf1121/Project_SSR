using System;
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
            if (brain.GetComponent<RangedTag>() != null)
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
            /* 공통 선계산 */
            Vector2 currentPos = tf.position;
            Vector2 targetPos = sPlayer.position;
            float dist = Vector2.Distance(currentPos, targetPos);
            /* ───────── ① 원거리 전용 가드 (맨 위) ───────── */
            if (isRanged)
            {
                /* ── 공통 파동 계산 ── */
                const float FS = 1.5f, AMP = 0.2f;
                float bobY = Mathf.Sin(Time.time * FS) * AMP;

                /* 0) 추적 범위 초과 → Idle */
                if (dist > brain.Stats.ChaseRange)
                {
                    rb.velocity = new Vector2(0f, bobY);
                    brain.ChangeState(StateID.Idle);
                    return;
                }

                /* 1) 공격 범위 안 → 정지 + AttackState */
                if (dist <= statData.attackRange)
                {
                    rb.velocity = new Vector2(0f, bobY);
                    brain.ChangeState(StateID.Attack);   // RangeAttackState
                    return;
                }

                /* 2) 이동 사격 구간 (attackRange ~ chaseRange) */
                if (Time.time - lastShot >= statData.rangedCooldown)
                {
                    Shoot();
                    lastShot = Time.time;
                }

                /* 3) 플레이어를 향해 추격 + 파동 */
                Vector2 dir = (targetPos - currentPos).normalized;
                float speed = brain.Stats.MoveSpeed *
                              (dist < brain.Stats.ChargeRange ? statData.rushMultiplier : 1f);

                rb.velocity = new Vector2(dir.x * speed, bobY);
                /* 얼굴 방향 정렬 */
                if (Mathf.Abs(dir.x) > 0.05f)
                {
                    Vector3 sc = tf.localScale;
                    sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x);   // 오른쪽 = +
                    tf.localScale = sc;
                }
                return;      // 근접형 로직으로 내려가지 않음
            }
            /* ───────── ② 이하 = 근거리/일반 추적 로직 ───────── */
             

            if (dist <= brain.Stats.ChaseRange)
            {
                Vector2 dir = (targetPos - currentPos).normalized;
                float speed = brain.Stats.MoveSpeed *
                              (dist < brain.Stats.ChargeRange ? statData.rushMultiplier : 1f);
                rb.velocity = dir * speed;

                if (Mathf.Abs(dir.x) > 0.05f)
                {
                    Vector3 sc = tf.localScale;
                    sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x);   // 부호 통일
                    tf.localScale = sc;
                }
            }
            else
            {
                float bobY = Mathf.Sin(Time.time * 1.5f) * 0.2f;
                rb.velocity = new Vector2(0f, bobY);
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
