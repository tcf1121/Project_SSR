using UnityEngine;
using UnityEditor;

namespace PHG
{
    [ExecuteAlways]
    [RequireComponent(typeof(MonsterBrain))]
    public class MonsterDebugGizmos : MonoBehaviour
    {
        public bool showGizmos = true;

        private MonsterBrain _brain;
        private JumpMove _jumper;
        private Transform _muzzle;
        private Transform _player;

#if UNITY_EDITOR
        private void OnValidate()
        {
            SceneView.RepaintAll();
        }
#endif

        private void OnEnable()
        {
            _brain = GetComponent<MonsterBrain>();
            _jumper = GetComponent<JumpMove>();
            _muzzle = transform.Find("MuzzlePoint");
            _player = GameObject.FindWithTag("Player")?.transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            if (_brain == null) _brain = GetComponent<MonsterBrain>();
            if (_jumper == null) _jumper = GetComponent<JumpMove>();
            if (_muzzle == null) _muzzle = transform.Find("MuzzlePoint");
            if (_player == null) _player = GameObject.FindWithTag("Player")?.transform;

            // ✔ MonsterStatEntry 유실 시 자동 복구 시도
            if (_brain != null && _brain.statData == null)
            {
                Debug.LogWarning($"[DebugGizmos] {name}의 statData가 NULL입니다. 복구 시도 중...");

                if (_brain.AllStatData != null)
                {
                    var recovered = _brain.AllStatData.GetStatEntry(_brain.MonsterType);
                    if (recovered != null)
                    {
                        // MonsterBrain 내부에서 Set 없이 접근 가능하도록 public set 허용 필요 (아래 참고)
                        var prop = typeof(MonsterBrain).GetProperty("statData");
                        prop?.SetValue(_brain, recovered);
                        Debug.Log($"[DebugGizmos] {name}의 statData가 성공적으로 복구되었습니다.");
                    }
                    else
                    {
                        Debug.LogError($"[DebugGizmos] {name}의 statData 복구 실패: AllMonsterStatData에 {_brain.MonsterType} 없음.");
                        return;
                    }
                }
                else
                {
                    Debug.LogError($"[DebugGizmos] {name}의 statData 복구 실패: AllStatData 연결이 없음.");
                    return;
                }
            }

            if (_brain == null || _brain.statData == null || _brain.sensor == null || _brain.wallSensor == null)
                return;

            var statData = _brain.statData;
            float dir = Mathf.Sign(transform.localScale.x);
            Vector3 p = transform.position;

            Gizmos.color = Color.cyan;
            Vector3 groundCheckPos = _brain.sensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
            Gizmos.DrawWireSphere(groundCheckPos, 0.1f);

            Gizmos.color = new Color(0f, 0.5f, 0.5f);
            Gizmos.DrawLine(_brain.sensor.position, _brain.sensor.position + Vector3.right * dir * 0.10f);
            Gizmos.DrawLine(_brain.sensor.position, _brain.sensor.position + Vector3.right * dir * 0.35f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_brain.wallSensor.position, 0.05f);
            float commonWallCheckDist = 0.2f;
            Gizmos.DrawLine(_brain.wallSensor.position, _brain.wallSensor.position + Vector3.right * dir * commonWallCheckDist);

            if (_jumper != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.1f, transform.position + Vector3.up * 1.5f);
            }

            if (statData.readyRange > 0f)
            {
                Handles.color = new Color(0f, 1f, 0f, 0.35f);
                Handles.DrawWireDisc(p, Vector3.forward, statData.readyRange);
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

            if (GetComponent<RangedTag>() != null && _muzzle != null && _player != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 from = _muzzle.position;
                Vector3 to = _player.position;
                Vector3 dir2 = (to - from).normalized;

                Gizmos.DrawLine(from, from + dir2 * statData.attackRange);
                Gizmos.DrawWireSphere(from + dir2 * statData.attackRange, 0.1f);
            }
        }
#endif
    }
}