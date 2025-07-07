using UnityEngine;


public class IdleState : IState
{
    private Monster _monster;
    private readonly MonsterStatEntry _statData;

    public IdleState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
    }

    public void Enter()
    {
        _monster.Rigid.velocity = Vector2.zero;

        if (_monster.Brain.StatData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Idle);
    }
    public void Tick()
    {
        if (_statData == null) return;             // 안전 방어


        /* ── 순찰 플래그가 켜져 있으면 즉시 Patrol ── */
        if (_statData.usePatrol)
        {
            _monster.ChangeState(StateID.Patrol);
            return;
        }

        /* ── 비행 유닛은 Idle 상태에서도 부유 애니메이션 ── */
        if (_statData.isFlying)
        {
            float floatSpeed = 2.5f;
            float floatAmplitude = 0.3f;
            _monster.Rigid.velocity = new Vector2(0f,
                           Mathf.Sin(Time.time * floatSpeed) * floatAmplitude);
        }
        else
        {
            _monster.Rigid.velocity = Vector2.zero;
        }

        /* ── 플레이어 감지 → Chase / FloatChase 전환 ── */

        if (!_statData.isRanged)
        {
            if (_monster.PlayerInRange(_statData.patrolRange)==true)
            {
                if (_statData.isFlying) _monster.ChangeState(StateID.FloatChase);
                else _monster.ChangeState(StateID.Chase);
                return;
            }
        }
        else if (_statData.isRanged)
        {
            if (_monster.PlayerInRange(_statData.readyRange))
            {
                _monster.ChangeState(StateID.AimReady);
                return;
            }
        }
    }

    public void Exit() => _monster.Rigid.velocity = Vector2.zero;
}