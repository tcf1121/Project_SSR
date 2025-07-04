using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    public interface IAttackBehavior
    {
        void Execute(MonsterBrain brain);
    }
}
