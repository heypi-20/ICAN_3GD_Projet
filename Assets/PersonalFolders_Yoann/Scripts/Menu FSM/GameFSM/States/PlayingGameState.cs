using UnityEngine;


public class PlayingGameState : GameState
{
    public override void Enter()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public override void Tick()
    {
       //todo Pause game when pressing escape
       if (Input.GetKeyDown(KeyCode.Escape))
       {
           fsm.ChangeState(GetComponent<PauseGameState>());
       }
    }
}