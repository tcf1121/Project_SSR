using UnityEngine;


/// <summary>
/// ▷ 원거리 몬스터 전용 대기 상태
/// 플레이어가 readyRange 안에 들어오면 정지‧조준만 하고,
/// attackRange 안으로 들어오면 RangeAttackState 로 전환.
/// </summary>
public class AimReadyState : IState
{
    private readonly Monster _monster;
    private readonly MonsterStatEntry _statData;

    float sqReady, sqAttack;   // 거리 비교용 제곱값

    public AimReadyState(Monster monster)
    {
        _monster = monster;
        _statData = monster.Brain.StatData;

        /* 계산 캐싱 */
        sqReady = _statData.readyRange * _statData.readyRange;
        sqAttack = _statData.attackRange * _statData.attackRange;
    }
    public void Enter()
    {

        if (_statData.hasIdleAnim)
            _monster.PlayAnim(AnimNames.Idle);
        if (_monster.Brain.IsGrounded() && !_monster.Brain.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;
    }

    public void Tick()
    {
        if (_monster.Target == null)
        {
            _monster.ChangeState(StateID.Patrol);
            return;
        }

        float sqDist = (_monster.Target.position - _monster.Transfrom.position).sqrMagnitude;

        /* 10타일 밖 → Patrol 복귀 */
        if (sqDist > sqReady)
        {
            _monster.ChangeState(StateID.Patrol);
            return;
        }

        /* 사정거리 진입 → Attack 전환 */
        if (sqDist <= sqAttack)
        {
            _monster.ChangeState(StateID.Attack);
            return;
        }


        /* 정지‧방향 전환만 수행 */
        if (_monster.Brain.IsGrounded() && !_monster.Brain.IsMidJump)
            _monster.Rigid.velocity = Vector2.zero;
        int dir = _monster.Target.position.x > _monster.Transfrom.position.x ? 1 : -1;
        _monster.Transfrom.localScale = new Vector3(Mathf.Abs(_monster.Transfrom.localScale.x) * dir,
        _monster.Transfrom.localScale.y, _monster.Transfrom.localScale.z);
    }

    public void Exit() => _monster.Rigid.velocity = Vector2.zero;
}