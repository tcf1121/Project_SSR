using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ←→ 왕복 순찰 → 플레이어 감지 시 Chase / Attack 전환
    /// </summary>
    public class PatrolState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly Transform groundSensor;
        private readonly Transform wallSensor;
        private readonly MonsterStatEntry statData;

        private int dir = 1;
        private const float floorCheckDist = 0.4f;
        private const float wallCheckDist = 0.2f;

        public PatrolState(MonsterBrain brain)
        {
            this.brain = brain;
            this.rb = brain.rb;
            this.tf = brain.tf;
            this.groundSensor = brain.sensor;
            this.wallSensor = brain.wallSensor;
            this.statData = brain.StatData;
        }

        public void Enter()
        {

            if (brain.StatData.hasIdleAnim)
                brain.PlayAnim(AnimNames.Walk);

            dir = tf.localScale.x >= 0f ? 1 : -1;
        }

        public void Tick()
        {
            if (statData == null) return;

            rb.velocity = new Vector2(dir * statData.moveSpeed, rb.velocity.y);

            Vector2 groundCheckPos = groundSensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
            float groundCheckRadius = 0.1f;

            bool noFloor = !Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, LayerMask.GetMask("Ground", "Platform"));
            bool hitWall = Physics2D.Raycast(wallSensor.position, Vector2.right * dir, wallCheckDist, LayerMask.GetMask("Ground", "Platform"));

            if (noFloor || hitWall)
            {
                dir *= -1;
                Vector3 scale = tf.localScale;
                scale.x = Mathf.Abs(scale.x) * dir;
                tf.localScale = scale;
            }

            // 0순위: 비행 원거리형 유닛 전용 추적 (FloatChase)
            // patrolRange와 readyRange를 모두 만족할 때 (patrolRange는 가장 넓은 범위, readyRange는 공격 준비 범위)
            if (brain.IsRanged && brain.IsFlying && PlayerInRange(statData.readyRange) && PlayerInRange(statData.patrolRange))
            {
                brain.ChangeState(StateID.FloatChase);
                return;
            }

            // 1순위: 추적 조건 — 모든 유닛 공통
            if (PlayerInRange(statData.patrolRange))
            {
                brain.ChangeState(StateID.Chase);
                return;
            }

            // 2순위: 공격 조건
            if (PlayerInRange(statData.attackRange))
            {
                brain.ChangeState(StateID.Attack);
                return;
            }

            // 3순위: readyRange는 상태 전이 없음 (조준만)
            if (brain.IsRanged &&
                brain.Sm.CurrentStateID != StateID.Chase &&
                PlayerInRange(statData.readyRange))
            {
                FacePlayer();
                rb.velocity = Vector2.zero;
                brain.ChangeState(StateID.AimReady);
                return;
            }
            if (brain.CanClimbLadders && PlayerInRange(statData.patrolRange))
            {
                var pl = GameObject.FindWithTag("Player")?.transform;
                if (pl != null)
                    brain.Climber?.TryFindAndClimb(dir, pl.position);
            }
        }

        public void Exit() => rb.velocity = Vector2.zero;

        private bool PlayerInRange(float range)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return false;
            return Vector2.Distance(tf.position, player.transform.position) <= range;
        }

        private void FacePlayer()
        {
            var pl = GameObject.FindWithTag("Player")?.transform;
            if (pl == null) return;

            int sign = pl.position.x > tf.position.x ? 1 : -1;
            Vector3 s = tf.localScale;
            s.x = Mathf.Abs(s.x) * sign;
            tf.localScale = s;
        }
    }
}