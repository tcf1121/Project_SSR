using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 몬스터 FSM 상태 식별자
/// </summary>
/// 
namespace PHG
{
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
        Dead
    }
}
