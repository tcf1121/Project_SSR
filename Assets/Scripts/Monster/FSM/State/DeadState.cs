using System.Collections;
using UnityEngine;
using Utill;


public class DeadState : IState
{
    private readonly Monster _monster;

    public DeadState(Monster monster)
    {
        _monster = monster;
    }

    public void Enter()
    {
        _monster.Death();
    }

    public void Tick() { }

    public void Exit() { }


}