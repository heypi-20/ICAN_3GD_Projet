using UnityEngine;

public class S_InGameState : S_IGameState
{
    private S_GameFlowController _fsm;
    public S_InGameState(S_GameFlowController fsm) { _fsm = fsm; }

    public void OnEnter(params object[] args)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnTick() { }

    public void OnExit()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}