using UnityEngine;
using UnityEngine.SceneManagement;
public class S_InGameState : MonoBehaviour,S_IGameState
{
    public void OnEnter(params object[] args)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnTick()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            S_GameFlowController.Instance.FireEvent(S_GameEvent.ReturnMainMenu);
        }
    }

    public void OnExit()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.UnloadSceneAsync("LD_ToutPlat_ScenePrincipal");

    }
}