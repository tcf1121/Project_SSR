using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���� FSM ���� �ĺ���
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
