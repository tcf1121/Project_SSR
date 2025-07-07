using UnityEngine;

/// <summary>
/// ←→ 왕복 순찰 → 플레이어 감지 시 Chase / Attack 전환
/// </summary>
public class PatrolState : IState
{
    private Monster _monster;
    private readonly MonsterStatEntry _statData;

    private int dir = 1;
    private const float floorCheckDist = 0.4f;
    private const float wallCheckDist = 0.2f;

    public PatrolState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;
    }

    public void Enter()
    {

        if (_monster.Brain.StatData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Walk);
        _monster.Rigid.velocity = Vector2.zero;
        dir = _monster.Transfrom.localScale.x >= 0f ? 1 : -1;
    }

    public void Tick()
    {
        if (_statData == null) return;

        _monster.Rigid.velocity = new Vector2(dir * _monster.MonsterStats.MoveSpeed, _monster.Rigid.velocity.y);

        Vector2 groundCheckPos = _monster.GroundSensor.position + Vector3.right * dir * 0.3f + Vector3.down * 0.1f;
        float groundCheckRadius = 0.1f;

        bool noFloor = !Physics2D.OverlapCircle(groundCheckPos, groundCheckRadius, _statData.groundMask);
        bool hitWall = Physics2D.Raycast(_monster.WallSensor.position, Vector2.right * dir, wallCheckDist, _statData.wallMask);

        if (noFloor || hitWall)
        {
            dir *= -1;
            Vector3 scale = _monster.Transfrom.localScale;
            scale.x = Mathf.Abs(scale.x) * dir;
            _monster.Transfrom.localScale = scale;
        }

        // 0순위: 비행 원거리형 유닛 전용 추적 (FloatChase)
        // patrolRange와 readyRange를 모두 만족할 때 (patrolRange는 가장 넓은 범위, readyRange는 공격 준비 범위)
        if (_statData.isRanged && _statData.isFlying && _monster.PlayerInRange(_statData.readyRange) && _monster.PlayerInRange(_statData.patrolRange))
        {
            _monster.ChangeState(StateID.FloatChase);
            return;
        }

        // 1순위: 추적 조건 — 모든 유닛 공통
        else if (_monster.PlayerInRange(_statData.patrolRange))
        {
            _monster.ChangeState(StateID.Chase);
            return;
        }

        // 2순위: 공격 조건
        else if (_monster.PlayerInRange(_statData.attackRange))
        {
            _monster.ChangeState(StateID.Attack);
            return;
        }

        // 3순위: readyRange는 상태 전이 없음 (조준만)
        else if (_statData.isRanged &&
            _monster.Brain.StateMachine.CurrentStateID != StateID.Chase &&
            _monster.PlayerInRange(_statData.readyRange))
        {
            FacePlayer();
            _monster.Rigid.velocity = Vector2.zero;
            _monster.ChangeState(StateID.AimReady);
            return;
        }

        if (_statData.enableLadderClimb && _monster.PlayerInRange(_statData.patrolRange))
        {
            if (_monster.Target != null)
                _monster.Brain.Climber?.TryFindAndClimb(dir);
        }
    }

    public void Exit() => _monster.Rigid.velocity = Vector2.zero;



    private void FacePlayer()
    {
        if (_monster.Target == null) return;

        int sign = _monster.Target.position.x > _monster.transform.position.x ? 1 : -1;
        Vector3 s = _monster.transform.localScale;
        s.x = Mathf.Abs(s.x) * sign;
        _monster.transform.localScale = s;
    }
}