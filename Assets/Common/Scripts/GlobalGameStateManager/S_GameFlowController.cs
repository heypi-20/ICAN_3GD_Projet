using System;
using System.Collections.Generic;
using UnityEngine;

public class S_GameFlowController : MonoBehaviour
{
    public static S_GameFlowController Instance { get; private set; }
    private S_IGameState _currentState;
    private Dictionary<Type, S_IGameState> _states = new Dictionary<Type, S_IGameState>();
    
    public event Action<Type> OnStateChanged;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterStates();
        }
    }

    private void RegisterStates()
    {
        _states[typeof(S_MainMenuState)]  = new S_MainMenuState(this);
        _states[typeof(S_LoadingState)]   = new S_LoadingState(this);
        _states[typeof(S_InGameState)]    = new S_InGameState(this);
        _states[typeof(S_GamePauseState)] = new S_GamePauseState(this);
        _states[typeof(S_GameOverState)]  = new S_GameOverState(this);

        ChangeState<S_MainMenuState>();
    }

    private void Update()
    {
        _currentState?.OnTick();
    }

    public void FireEvent(S_GameEvent evt, params object[] args)
    {
        switch (evt)
        {
            case S_GameEvent.ReturnMainMenu:
                ChangeState<S_MainMenuState>();
                break;
            case S_GameEvent.LevelSelected:
            case S_GameEvent.Restart:
                ChangeState<S_LoadingState>(args);
                break;
            case S_GameEvent.GameStart:
                ChangeState<S_InGameState>();
                break;
            case S_GameEvent.PauseGame:
                ChangeState<S_GamePauseState>();
                break;
            case S_GameEvent.GameOver:
                ChangeState<S_GameOverState>(args);
                break;
            default:
                Debug.LogWarning("Unhandled S_GameEvent: " + evt);
                break;
        }
    }

    private void ChangeState<T>(params object[] args) where T : S_IGameState
    {
        _currentState?.OnExit();
        _currentState = _states[typeof(T)];
        _currentState.OnEnter(args);
        OnStateChanged?.Invoke(typeof(T));
    }
}
