using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AttackState : IState
{
    void Attack();
    void FinishAttack();
    void CancelAttack();

}
