using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mob.State;

public class FiniteStateMachine
{
    private MStatable curState;

    public FiniteStateMachine(MStatable initState)
    {
        curState = initState;
        ChangeState(curState);
    }

    public void ChangeState(MStatable nextstate)
    {
        if(curState == nextstate)
        {
            return;
        }
        if(curState == null)
        {
            curState.OnStateExit();
        }

        curState = nextstate;
        nextstate.OnStateExit();
    }

    public void UpdateState()
    {
        if (curState != null)
        {
            curState.OnStateUpdate();
        }
    }
}
