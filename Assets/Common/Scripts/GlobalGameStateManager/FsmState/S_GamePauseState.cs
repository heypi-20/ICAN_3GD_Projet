using UnityEngine;

public class S_GamePauseState : S_IGameState
{
    private S_GameFlowController _fsm;
    public S_GamePauseState(S_GameFlowController fsm) { _fsm = fsm; }

    public void OnEnter(params object[] args)
    {
        //pause game, show pause menu and cursor
    }

    public void OnTick() { }

    public void OnExit()
    {
        //resume game, hide pause menu and cursor
    }
}