using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    public class DeadState : IState
    {
        private readonly MonsterBrain brain;
        public DeadState(MonsterBrain brain) => this.brain = brain;

        public void Enter() => Object.Destroy(brain.gameObject);
        public void Tick() { }
        public void Exit() { }


    }

}