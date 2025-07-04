using UnityEngine;

namespace PHG
{
    public class FloatChaseState : IState
    {
        /* ───── refs ───── */
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private static Transform sPlayer;        // 플레이어 캐싱
        private readonly MonsterStatEntry statData;

        /* 원거리 전용 */
        private readonly bool isRanged;
        private readonly Transform muzzle;

        public FloatChaseState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.rb;
            tf = brain.tf;
            statData = brain.StatData;

            /* Tag 의존 제거 —> Ability Flag 사용 */
            isRanged = brain.IsRanged;
            if (isRanged)
                muzzle = brain.Muzzle;  // 원거리 전용 총구
        }

        /* -------------------------------------------------------------- */
        #region IState
        public void Enter()
        {
            rb.gravityScale = 0f;      // 비행: 중력 제거
            rb.velocity = Vector2.zero;

            Debug.Log($"(GameObject: {brain.gameObject.name}) FloatChaseState Enter");
            if (sPlayer == null)
                sPlayer = GameObject.FindWithTag("Player")?.transform;
            Debug.Log($"(GameObject: {brain.gameObject.name}) 플레이어 캐싱: {sPlayer?.name}");

            // Note: hasIdleAnim이 false이면 Walk 애니메이션이 재생되지 않을 수 있습니다.
            // MonsterStatEntry에서 hasIdleAnim을 true로 설정했는지 확인하세요.
            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Walk);
            Debug.Log($"(GameObject: {brain.gameObject.name}) Walk 애니메이션 재생 (hasIdleAnim 상태: {brain.StatData.hasIdleAnim})");
        }

        public void Tick()
        {
            // 1. 플레이어 없으면 Patrol로 복귀 (또는 Idle)
            if (sPlayer == null)
            {
                brain.ChangeState(StateID.Patrol); // 또는 StateID.Idle
                Debug.Log($" (GameObject: {brain.gameObject.name}) FloatChaseState: 플레이어 없음, PatrolState로 전환");
                return;
            }

            float dist = Vector2.Distance(tf.position, sPlayer.position);
            Vector2 toPl = (sPlayer.position - tf.position); // 현재 이 부분은 아래에서 사용하지 않으면 제거 가능

            // 1. 플레이어가 추격 최대 범위를 벗어나면 Idle로 전환 (최우선 순위)
            if (dist > statData.chaseRange) // Rinnegan: dist > 8
            {
                Vector2 dir = toPl.normalized;
                brain.ChangeState(StateID.Idle);
                Orient(dir);
             //   Debug.Log($" (GameObject: {brain.gameObject.name}) FloatChaseState: 플레이어가 추격 범위를 벗어남 ({dist:F2} > {statData.chaseRange:F2}), IdleState로 전환");
                return;
            }

            // 2. 공격 범위 내에 있으면 공격 상태로 전환
            if (dist <= statData.attackRange) // Rinnegan: dist <= 3
            {
                // isRanged가 true이므로 AttackState로 전환 (RangeAttackState)
                brain.ChangeState(StateID.Attack);
              //  Debug.Log($" (GameObject: {brain.gameObject.name}) FloatChaseState: 공격 범위 진입 ({dist:F2} <= {statData.attackRange:F2}), AttackState로 전환");
                return;
            }

            // 3. 공격 범위는 벗어났지만 (dist > attackRange), 조준 대기 범위 내에 있으면 AimReady 상태로 전환
            // Rinnegan: (3 < dist <= 12) 이면서 chaseRange (8) 내에 있음 -> (3 < dist <= 8)
            if (isRanged && dist <= statData.readyRange)
            {
                brain.ChangeState(StateID.AimReady);
            //    Debug.Log($" (GameObject: {brain.gameObject.name}) FloatChaseState: 조준 대기 범위 진입 ({dist:F2} <= {statData.readyRange:F2}), AimReadyState로 전환");
                return;
            }

            // 4. (이 블록은 Rinnegan 스탯으로는 도달하지 않아야 함)
            // 즉, isRanged가 false인 근접 비행 몬스터이거나,
            // (readyRange < dist <= chaseRange)인 원거리 몬스터라면 이곳에서 추격 로직을 수행해야 합니다.
            // 현재 Rinnegan (readyRange 12 > chaseRange 8) 스탯에서는 위 if문에서 모두 처리되므로 이 else 블록은 도달하지 않습니다.
            // 만약 도달한다면, 스탯 설정 또는 로직에 문제가 있는 것입니다.
            else
            {
                Vector2 dir = toPl.normalized;
               float speed = statData.moveSpeed;
                rb.velocity = dir * speed;
                Orient(dir);
                //Debug.Log($" (GameObject: {brain.gameObject.name}) FloatChaseState: 플레이어에게 접근 중 (현재 스탯으로는 도달 불가 로직?), 속도: {speed:F2}");
            }
        }
        public void Exit() => rb.velocity = Vector2.zero;
        #endregion

        /* -------------------------------------------------------------- */
        #region helpers
        void Orient(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > 0.05f)
            {
                Vector3 sc = tf.localScale;
                sc.x = Mathf.Abs(sc.x) * Mathf.Sign(dir.x);
                tf.localScale = sc;
            }
        }
        #endregion
    }
}