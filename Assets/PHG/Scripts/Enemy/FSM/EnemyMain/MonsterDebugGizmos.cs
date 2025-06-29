using UnityEngine;
using UnityEditor; // Handles 사용을 위해 필요

namespace PHG
{
    [ExecuteAlways]
    [RequireComponent(typeof(MonsterBrain))]
    public class MonsterDebugGizmos : MonoBehaviour
    {
        /* ───────── 레퍼런스 캐싱 ───────── */
        private MonsterBrain brain;
        private JumpMove jumper;
        private Transform muzzle;
        private Transform player;

        private void OnEnable()
        {
            brain = GetComponent<MonsterBrain>();
            jumper = GetComponent<JumpMove>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // brain, brain.sensor (ground checker), brain.wallSensor, statData 모두 할당되었는지 확인
            if (brain == null || brain.sensor == null || brain.wallSensor == null || brain.StatData == null) return;

            var statData = brain.StatData;

            muzzle = transform.Find("MuzzlePoint");
            player = GameObject.FindWithTag("Player")?.transform;

            float dir = Mathf.Sign(transform.localScale.x);

            // groundSensor (기존 brain.sensor) 위치의 바닥 감지 영역 시각화
            Gizmos.color = Color.cyan;
            Vector3 groundCheckPos = brain.sensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
            Gizmos.DrawWireSphere(groundCheckPos, 0.1f);

            // 기존 groundSensor 관련 라인 (색깔 변경으로 구분)
            Gizmos.color = new Color(0f, 0.5f, 0.5f); // 어두운 시안색
            Gizmos.DrawLine(brain.sensor.position, brain.sensor.position + Vector3.right * dir * 0.10f);
            Gizmos.DrawLine(brain.sensor.position, brain.sensor.position + Vector3.right * dir * 0.35f);

            // wallSensor (새로 추가된 벽 감지 센서) 위치 및 레이 시각화
            Gizmos.color = Color.magenta; // 벽 감지용은 마젠타 색상으로 구분
            Gizmos.DrawWireSphere(brain.wallSensor.position, 0.05f); // wallSensor의 원점 표시

            // PatrolState와 ChaseState에서 사용하는 벽 감지 거리 중 일반적인 값을 사용
            float commonWallCheckDist = 0.2f;
            Gizmos.DrawLine(brain.wallSensor.position, brain.wallSensor.position + Vector3.right * dir * commonWallCheckDist);


            if (jumper != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                                transform.position + Vector3.up * 1.5f);
            }

            Vector3 p = transform.position;

            if (statData.readyRange > 0f) //사격범위
            {
                Handles.color = new Color(0f, 1f, 0f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.readyRange); ;
            }

            if (statData.patrolRange > 0f)
            {
                Handles.color = new Color(0f, 0.6f, 1f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.patrolRange);
            }
            if (statData.chaseRange > 0f)
            {
                Handles.color = new Color(1f, 0.85f, 0f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.chaseRange);
            }
            if (statData.attackRange > 0f)
            {
                Handles.color = new Color(1f, 0f, 0f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.attackRange);
            }
            if (statData.chargeRange > 0f)
            {
                Handles.color = new Color(1f, 0.2f, 1f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.chargeRange);
            }

            if (GetComponent<RangedTag>() != null && muzzle != null && player != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 from = muzzle.position;
                Vector3 to = player.position;
                Vector3 dir2 = (to - from).normalized;

                Gizmos.DrawLine(from, from + dir2 * statData.attackRange);
                Gizmos.DrawWireSphere(from + dir2 * statData.attackRange, 0.1f);
            }
        }
#endif
    }
}