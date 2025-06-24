using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PHG;
public class AttackState : IState
{
    private readonly MonsterBrain brain;
    public AttackState(MonsterBrain brain) => this.brain = brain;

    public void Enter() => brain.ChangeState(StateID.Chase); // 즉시 추격으로 복귀
    public void Tick() { }                                  // 비워둠
    public void Exit() { }
}
