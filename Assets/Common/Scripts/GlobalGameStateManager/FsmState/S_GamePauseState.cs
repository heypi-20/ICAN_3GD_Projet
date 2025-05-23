using UnityEngine;

public class S_GamePauseState : MonoBehaviour,S_IGameState
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