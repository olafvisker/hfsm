using System;
using System.Collections.Generic;

public class State
{
    private string name;
    private Action onEnter;
    private Action onTick;
    private Action onExit;
    private List<Tuple<State, Func<bool>>> transitions = new List<Tuple<State, Func<bool>>>();
    private List<State> parents = new List<State>();
    private State entryState;

    public string Name { get => name; set => name = value; }

    public State() { onEnter = Start; onTick = Update; onExit = End; }
    public State(Action onTick) { this.onTick = onTick; }
    public State(Action onEnter, Action onTick) : this(onTick) { this.onEnter = onEnter; }
    public State(Action onEnter, Action onTick, Action onExit) : this(onEnter, onTick) { this.onExit = onExit; }
    public State(params State[] children)
    {
        entryState = children[0];
        foreach (State child in children)
            child.AddParent(this);
    }

    public State To(State to) { transitions.Add(new Tuple<State, Func<bool>>(to, Finished)); return to; }
    public State To(State to, Func<bool> condition) { transitions.Add(new Tuple<State, Func<bool>>(to, condition)); return to; }
    public void AddParent(State parent) { parents.Add(parent); }
    public void SetEntryState(State state) { entryState = state; }

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

        foreach (Tuple<State, Func<bool>> t in transitions)
            if (t.Item2()) return t.Item1;

        return null;
    }

    public void Enter() { if (onEnter != null) onEnter(); }
    public void Tick() { onTick(); }
    public void Exit() { if (onExit != null) onExit(); }

    public virtual void Start() { }
    public virtual void Update() { }
    public virtual void End() { }
    public virtual bool Finished() { return true; }
}


public class HFSM
{
    private State currentState;

    public void SetState(State state) { currentState = state; }
    public State To(State from, State to) { from.To(to); return to; }
    public State To(State from, State to, Func<bool> condition) { from.To(to, condition); return to; }

    public void Tick()
    {
        State entry = currentState.GetFinalEntryState();
        if (entry != null) currentState = entry;

        currentState.Tick();

        State to = currentState.GetTransitionState();
        if (to != null)
        {
            currentState.Exit();
            currentState = to;
            currentState.Enter();
        }
    }
}