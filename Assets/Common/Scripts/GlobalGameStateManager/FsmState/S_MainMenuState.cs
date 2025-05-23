using UnityEngine.SceneManagement;
using UnityEngine;

public class S_MainMenuState : MonoBehaviour,S_IGameState
{
    public GameObject _panel;

    public void OnEnter(params object[] args)
    {
        _panel.SetActive(true);
    }

    public void OnTick() { }

    public void OnExit()
    {
        _panel.SetActive(false);
    }
}