using System;
using System.Collections.Generic;

public class State
{
    private string name = "state";
    private Action onEnter;
    private Action onTick;
    private Action onExit;
    private List<Tuple<State, List<Func<bool>>>> transitions = new List<Tuple<State, List<Func<bool>>>>();
    private List<State> parents = new List<State>();
    private State entryState;

    public string Name { get => name; set => name = value; }

    public State() { onEnter = Start; onTick = Update; onExit = End; }
    public State(string name) : this() { this.name = name; }
    public State(string name, Action onTick) : this(name) { this.onTick = onTick; }
    public State(string name, Action onEnter, Action onTick) : this(name, onTick) { this.onEnter = onEnter; }
    public State(string name, Action onEnter, Action onTick, Action onExit) : this(name, onEnter, onTick) { this.onExit = onExit; }
    public State(Action onTick) : this(onTick.Method.Name) { this.onTick = onTick; }
    public State(Action onEnter, Action onTick) : this(onTick) { this.onEnter = onEnter; }
    public State(Action onEnter, Action onTick, Action onExit) : this(onEnter, onTick) { this.onExit = onExit; }
    public State(params State[] children) : this(children[0].name + "*") { AddChildren(children); }

    public void AddChildren(params State[] children)
    {
        entryState = children[0];
        foreach (State child in children)
            child.AddParent(this);
    }
    public void AddParent(State parent) { parents.Add(parent); }
    public void SetEntryState(State state) { entryState = state; }

    public State To(State to) { return To(to, Finished); }
    public State To(State to, params Func<bool>[] conditions) { transitions.Add(new Tuple<State, List<Func<bool>>>(to, new List<Func<bool>>(conditions))); return to; }

    public State GetFinalEntryState()
    {
        if (entryState == null || entryState.entryState == null)
            return entryState;
        return entryState.GetFinalEntryState();
    }
    public State GetTransitionState()
    {
        foreach (State p in parents)
        {
            State to = p.GetTransitionState();
            if (to != null) return to;
        }

        foreach (Tuple<State, List<Func<bool>>> t in transitions)
        {
            bool transition = true;
            foreach (Func<bool> c in t.Item2)
                if (!c()) { transition = false; break; }
            if (transition) return t.Item1;
        }

        return null;
    }

    public void Enter() { if (onEnter != null) onEnter(); }
    public void Tick() { onTick(); }
    public void Exit() { if (onExit != null) onExit(); }

    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void End() { }
    protected virtual bool Finished() { return true; }
}

public class HFSM
{
    private State currentState;

    public State CurrentState { get => currentState; }

    public State To(State from, State to) { from.To(to); return to; }
    public State To(State from, State to, Func<bool> condition) { from.To(to, condition); return to; }

    public void SetState(State state)
    {
        if (currentState != null)
            currentState.Exit();
        currentState = state;
        currentState.Enter();
    }

    public void Tick()
    {
        State entry = currentState.GetFinalEntryState();
        if (entry != null) SetState(entry);
        currentState.Tick();
        State to = currentState.GetTransitionState();
        if (to != null) SetState(to);
    }
}
