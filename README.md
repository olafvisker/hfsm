# hfsm
A simple flexible Hierarchical Finite State Machine in C#.

<img src="https://github.com/olafvisker/hfsm/blob/main/img/hfsm.png" alt="HFSM Diagram" width="50%">

## documentation
States can be created either by instantiating a state directly and providing ```onEnter```, ```onTick``` and ```onExit``` functions, or by inhereting from ```State``` and overriding its ```Start```, ```Update```, ```End``` and ```Finished``` methods. States can also consist solely of other states effectively grouping them.

Transitions can be defined by overriding the ```Finished``` method or by providing a ```Func<bool>``` to the state condition in the constructor. If no condition is supplied transition is automatically triggered after one tick. When transitioning to grouped state the first state added is regarded as the entry state, unless a specific entry state is supplied. 

```C#
public class TimerState : State 
{
    float time = 0;
    float timer = 0;
    public TimerState(float time) { this.time = time; }
    public override void Start() { timer = 0; }
    public override void Update() { timer += Time.deltaTime; }
    public override void End() { }
    public override bool Finished() { return timer >= time;}        // Condition trigger
}

public class Program() 
{
    private static void Main(string[] args)
    {
        HFSM hfsm = new HFSM();                                     // Create new Hierarchical FSM
        
        State move = new State(Move);                               // Only contains onTick method
        State findRndWaypoint = new State(SetRandomWaypoint);
        State wait = new TimerState(10);                            // State by inheritance
        State flee = new State(PrepareFleeing, Flee, EndFleeing);
        State idle = new State(findRndWaypoint, move, wait);        // State consisting of other states
        
        hfsm.To(findRndWaypoint, move);                             // Transition defined through fsm without a condition (automatically triggered)
        move.To(wait, reachedLocation);                             // Transition directly defined with reachedLocation condition
        hfsm.To(wait, findRndWaypoint);                             // Condition implemented by overriding Finished() method
        hfsm.To(idle, flee, dangerClose);                           // Transition from group state idle to flee state
        hfsm.To(flee, idle, ()=>!dangerClose());                    // Transition from flee state to idle group state
    }
}
```
The ```To``` function returns the to be transitioned to state, this means that transitions can also be defined using chaining. 

```C#
findRndWaypoint
    .To(move)
    .To(wait, reachedLocation)
    .To(findRndWaypoint);

hfsm.To(idle, flee, dangerClose).To(idle, ()=>!dangerClose()); 
```
**enjoy!**
