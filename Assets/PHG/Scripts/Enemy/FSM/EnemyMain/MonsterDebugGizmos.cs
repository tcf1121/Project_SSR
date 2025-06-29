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
            if (brain == null || brain.sensor == null || brain.StatData == null) return;

            var statData = brain.StatData;

            muzzle = transform.Find("MuzzlePoint");
            player = GameObject.FindWithTag("Player")?.transform;

            float dir = Mathf.Sign(transform.localScale.x);

            // 센서 레이
            Gizmos.color = Color.cyan;
            Vector3 groundCheckPos = brain.sensor.position + Vector3.right * Mathf.Sign(transform.localScale.x) * 0.3f + Vector3.down * 0.1f;
            Gizmos.DrawWireSphere(groundCheckPos, 0.1f);

            Gizmos.color = new Color(1f, 0f, 1f);
            Gizmos.DrawLine(brain.sensor.position, brain.sensor.position + Vector3.right * dir * 0.10f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(brain.sensor.position, brain.sensor.position + Vector3.right * dir * 0.35f);

            if (jumper != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                                transform.position + Vector3.up * 1.5f);
            }

            Vector3 p = transform.position;

            if(statData.readyRange > 0f) //사격범위
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