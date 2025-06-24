using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PHG;
public class AttackState : IState
{
    private readonly MonsterBrain brain;
    public AttackState(MonsterBrain brain) => this.brain = brain;

    public void Enter() => brain.ChangeState(StateID.Chase); // ��� �߰����� ����
    public void Tick() { }                                  // �����
    public void Exit() { }
}
