using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PHG;
public class StateMachine
{
    private readonly Dictionary<StateID, IState> states = new();
    private IState current;

    public void Register(StateID id, IState state) => states[id] = state;

    public void ChangeState(StateID next)
    {
        if (!states.TryGetValue(next, out var target)) return;
        current?.Exit();
        current = target;
        current.Enter();
    }
    public void Tick() => current?.Tick();
}
