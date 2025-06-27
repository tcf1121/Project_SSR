using UnityEngine;

namespace PHG
{
    public class GreedIdleState : IState
    {
        readonly MonsterBrain brain;
        readonly Interactable interact;

        public GreedIdleState(MonsterBrain b, Interactable it)
        { brain = b; interact = it; }

        public void Enter() => interact.OnInteract += HandleInteract;
        public void Exit() => interact.OnInteract -= HandleInteract;
        public void Tick() { /* ´ë±â */ }

        void HandleInteract() => brain.ChangeState(StateID.Chase);
    }

}