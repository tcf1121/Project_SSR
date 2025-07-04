
/// <summary>
/// 몬스터 FSM 상태 식별자
/// </summary>
/// 
public enum StateID
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Climb,
    MoveToLadder,
    FloatChase,
    meleeAttack,
    TakeDamage,
    AimReady,
    Dead
}
