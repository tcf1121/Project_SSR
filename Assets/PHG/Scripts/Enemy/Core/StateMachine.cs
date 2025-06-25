using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PHG
{
    public class StateMachine
    {
        private readonly Dictionary<StateID, IState> states = new();
        private IState current;
        public StateID CurrentStateID { get; private set; }
        public void Register(StateID id, IState state) => states[id] = state;

        public void ChangeState(StateID next)
        {
            if (!states.TryGetValue(next, out var target)) return;
            current?.Exit();
            current = target;
            CurrentStateID = next; // 추가된 부분
            current.Enter();
        }
        public void Tick() => current?.Tick();
    }
}
