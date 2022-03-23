using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public Object Parent;
    public State CurrentState;
    
    public StateMachine(Object parent)
    {
        this.Parent = parent;
    }

    public void SetInitState(State initState)
    {
        CurrentState = initState;
    }

    public void Update()
    {
        CurrentState.TransitionCheck();
        CurrentState.Update();
    }
}
