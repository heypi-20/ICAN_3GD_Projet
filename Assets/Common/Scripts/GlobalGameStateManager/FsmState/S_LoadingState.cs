using System.Collections;
using UnityEngine;

public class S_LoadingState : S_IGameState
{
    private S_GameFlowController _fsm;
    private object _levelRef;

    public S_LoadingState(S_GameFlowController fsm) { _fsm = fsm; }

    public void OnEnter(params object[] args)
    {
        //load selected level
        //active load menu and anim wait x sec then active wait for click menu
    }

    public void OnTick() { }
    

    public void OnExit()
    {
        // hide load menu
    }
}