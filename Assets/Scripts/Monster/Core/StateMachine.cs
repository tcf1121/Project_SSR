using System.Collections.Generic;

public class StateMachine
{
    Monster _monster;
    MonsterBrain _brain => _monster.Brain;
    private readonly Dictionary<StateID, IState> states = new();
    public StateMachine(Monster monster)
    {
        _monster = monster;
    }
    private IState current;
    public StateID CurrentStateID { get; private set; }
    public void Register(StateID id, IState state) => states[id] = state;

    public void ChangeState(StateID next)
    {
        if (_monster.Brain.IsDead) return;
        if (!states.TryGetValue(next, out var target)) return;
        current?.Exit();
        current = target;
        CurrentStateID = next; // 추가된 부분
        current.Enter();
    }
    public void Tick() => current?.Tick();
}
