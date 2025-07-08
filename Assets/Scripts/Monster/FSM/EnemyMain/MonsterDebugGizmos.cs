using UnityEngine;
using UnityEditor;


[ExecuteAlways]
[RequireComponent(typeof(Monster))]
public class MonsterDebugGizmos : MonoBehaviour
{
    public bool showGizmos = true;

    private Monster _monster;
    private Transform _muzzle;
    private Transform _player;
    private LadderClimber _climber;

#if UNITY_EDITOR
    private void OnValidate() => SceneView.RepaintAll();
#endif

    private void OnEnable()
    {
        _monster = GetComponent<Monster>();
        _muzzle = transform.Find("MuzzlePoint");
        _player = GameObject.FindWithTag("Player")?.transform;
        _climber = _monster.Brain.Climber as LadderClimber;

        if (_monster != null && _climber != null)
            _climber.Init(_monster.Brain); //
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos || _monster == null || _monster.Brain.StatData == null) return;

        float dir = Mathf.Sign(transform.localScale.x);
        Vector3 p = transform.position;

        // ─ Sensor ─
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(_monster.GroundSensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f, 0.1f);

        Gizmos.color = new Color(0f, 0.5f, 0.5f);
        Gizmos.DrawLine(_monster.GroundSensor.position, _monster.GroundSensor.position + Vector3.right * dir * 0.10f);
        Gizmos.DrawLine(_monster.GroundSensor.position, _monster.GroundSensor.position + Vector3.right * dir * 0.35f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(_monster.WallSensor.position, 0.05f);
        Gizmos.DrawLine(_monster.WallSensor.position, _monster.WallSensor.position + Vector3.right * dir * 0.2f);

        // ─ Jump ─
        if (_monster.Brain.StatData.enableJump && _monster is IMonsterJumper jumper)
        {
            Vector2 pos = jumper.LastGroundCheckPos;
            float radius = jumper.GroundCheckRadius;
            Handles.color = jumper.IsGrounded() ? Color.green : Color.red;
            Handles.DrawWireDisc(pos, Vector3.forward, radius);
        }

        // ─ Ladder ─
    

        // ─ Ranges ─
        var stat = _monster.Brain.StatData;
        if (stat.readyRange > 0f) { Handles.color = new Color(0f, 1f, 0f, 0.35f); Handles.DrawWireDisc(p, Vector3.forward, stat.readyRange); }
        if (stat.patrolRange > 0f) { Handles.color = new Color(0f, 0.6f, 1f, 0.35f); Handles.DrawWireDisc(p, Vector3.forward, stat.patrolRange); }
        if (stat.chaseRange > 0f) { Handles.color = new Color(1f, 0.85f, 0f, 0.35f); Handles.DrawWireDisc(p, Vector3.forward, stat.chaseRange); }
        if (stat.attackRange > 0f) { Handles.color = new Color(1f, 0f, 0f, 0.35f); Handles.DrawWireDisc(p, Vector3.forward, stat.attackRange); }
        if (stat.chargeRange > 0f) { Handles.color = new Color(1f, 0.2f, 1f, 0.35f); Handles.DrawWireDisc(p, Vector3.forward, stat.chargeRange); }

        // ─ Ranged ─
        // ─ Ranged ─
        if (_monster.Brain.StatData.isRanged && _muzzle != null && _player != null)
        {
            Gizmos.color = Color.yellow;

            Vector3 from = _muzzle.position;
            Vector3 aimTarget = _player.position + Vector3.up * 0.25f; // 사격 방향 보정
            Vector3 dir2 = (aimTarget - from).normalized;

            Gizmos.DrawLine(from, from + dir2 * stat.attackRange);
            Gizmos.DrawWireSphere(from + dir2 * stat.attackRange, 0.1f);
        }
    }
#endif
}
