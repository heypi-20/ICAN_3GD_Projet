using UnityEngine;

public class S_InGameState : MonoBehaviour,S_IGameState
{
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