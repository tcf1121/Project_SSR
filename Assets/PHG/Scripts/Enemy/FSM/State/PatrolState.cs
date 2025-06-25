using UnityEngine;

namespace PHG
{
    /// <summary>
    /// ←→ 왕복으로 순찰하다가 플레이어가 가까워지면 추적 상태로 전환
    /// </summary>
    public class PatrolState : IState
    {
        private readonly MonsterBrain brain;
        private readonly Rigidbody2D rb;
        private readonly Transform tf;
        private readonly Transform sensor;
        private readonly LayerMask groundMask;

        private int dir = 1; // 1= -> , -1 = <-
        private const float floorCheckDist = 0.2f; //발아래 확인
        private const float wallCheckDist = 0.1f; //벽 확인 거리

        public PatrolState(MonsterBrain brain)
        {
            this.brain = brain;
            rb = brain.GetComponent<Rigidbody2D>(); //  몬스터 rigidbody 캐싱
            tf = brain.transform;

            // MonsterBrain 인스펙터에 Sensor, GrounMask 노출
            sensor = brain.sensor;
            groundMask = brain.groundMask;
        }
        /*========= IState 인터페이스 구현 =========*/
        public void Enter() { }   // 처음 들어올 때 할 일 없음
        public void Tick()
        {
            /** 1) 이동 코드 ---------------------------------- */
            //  dir(±1) * 이동속도로 X방향 속도 부여
            rb.velocity = new Vector2(dir * brain.Stats.MoveSpeed, rb.velocity.y);



            /* 낭떠러지, 벽감지 -> 방향 반전 */
            bool noFloor = !Physics2D.Raycast(sensor.position, Vector2.down, floorCheckDist, groundMask);
            bool hitWall = Physics2D.Raycast(sensor.position, Vector2.right * dir, wallCheckDist, groundMask);

            if (noFloor || hitWall)
            {
                dir *= -1; // 방향 반전
                tf.localScale = new Vector3(dir, 1, 1);
                //sensor가 항상 앞을 향하도록
            }
            /* 3) (옵션) 추적 전환 */
            // ① 공격 범위가 더 넓다면 먼저 발사
            if (PlayerInRange(brain.Stats.AttackRange))
            {
                rb.velocity = Vector2.zero;
                brain.ChangeState(StateID.Attack);
                return;
            }

            // ② 그보다 좁은 PatrolRange 안이면 Chase
            if (PlayerInRange(brain.Stats.PatrolRange))
            {
                brain.ChangeState(StateID.Chase);
                return;
            }
        }
        // 상태를 떠날 때 이동 중지
        public void Exit() => rb.velocity = Vector2.zero;

        /*──────────────── 헬퍼 메서드 ────────────────*/
        private bool PlayerInRange(float range)
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return false;
            return Vector2.Distance(tf.position, player.transform.position) <= range;
        }


    }

}