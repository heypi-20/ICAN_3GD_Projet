using System;
using UnityEngine;

public class S_UIStateBinder : MonoBehaviour
{
    public enum EState { MainMenu, Loading, InGame, Pause, GameOver }
    public EState boundState;

    private void OnEnable()  => S_GameFlowController.Instance.OnStateChanged += Handle;
    private void OnDisable() => S_GameFlowController.Instance.OnStateChanged -= Handle;

    private void Handle(Type newState)
    {
        bool show = 
            (newState == typeof(S_MainMenuState)    && boundState == EState.MainMenu)
            || (newState == typeof(S_LoadingState)     && boundState == EState.Loading)
            || (newState == typeof(S_InGameState)      && boundState == EState.InGame)
            || (newState == typeof(S_GamePauseState)   && boundState == EState.Pause)
            || (newState == typeof(S_GameOverState)    && boundState == EState.GameOver);

        gameObject.SetActive(show);
    }
}
