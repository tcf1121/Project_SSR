using UnityEngine;
using PHG;
#if UNITY_EDITOR
using UnityEditor;        // Handles
#endif

[ExecuteAlways]
[RequireComponent(typeof(MonsterBrain))]
public class MonsterDebugGizmos : MonoBehaviour
{
    /* ───────── 레퍼런스 캐싱 ───────── */
    private MonsterBrain brain;
    private JumpMove jumper;

    private void OnEnable()
    {
        brain = GetComponent<MonsterBrain>();
        jumper = GetComponent<JumpMove>();      // 없으면 null
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (brain == null || brain.sensor == null) return;

        float dir = Mathf.Sign(transform.localScale.x); // 바라보는 방향 (+/-)

        /*──────────────── 센서 레이 ────────────────*/
        // Patrol/Chase ① 바닥 확인
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(brain.sensor.position,
                        brain.sensor.position + Vector3.down * 0.20f);

        // Patrol/Chase ② 벽 확인
        Gizmos.color = new Color(1f, 0f, 1f);  // Magenta
        Gizmos.DrawLine(brain.sensor.position,
                        brain.sensor.position + Vector3.right * dir * 0.10f);

        // Chase        ③ wallAhead
        Gizmos.color = Color.red;
        Gizmos.DrawLine(brain.sensor.position,
                        brain.sensor.position + Vector3.right * dir * 0.35f);

        /*──────────────── JumpMove 레이 ─────────────*/
        if (jumper != null)
        {
            // 위쪽 점프 가능 플랫폼 감지용 디버그 (새 추가)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.1f,
                            transform.position + Vector3.up * 1.5f);
        }


        /*──────────────── 원형 감지 범위 ────────────*/
        /*──────────────── 원형 감지 범위 ────────────*/
        Vector3 p = transform.position;
        float patrolRange = brain.Stats.PatrolRange;
        float chaseRange = brain.Stats.ChaseRange;
        float attackRange = brain.Stats.AttackRange;

        if (patrolRange > 0f)
        {
            Handles.color = new Color(0f, 0.6f, 1f, 0.35f); // 하늘색
            Handles.DrawWireDisc(p, Vector3.forward, patrolRange);
        }
        if (chaseRange > 0f)
        {
            Handles.color = new Color(1f, 0.85f, 0f, 0.35f); // 노랑
            Handles.DrawWireDisc(p, Vector3.forward, chaseRange);
        }
        if (attackRange > 0f)
        {
            Handles.color = new Color(1f, 0f, 0f, 0.35f); // 빨강
            Handles.DrawWireDisc(p, Vector3.forward, attackRange);
        }

    }
#endif
}