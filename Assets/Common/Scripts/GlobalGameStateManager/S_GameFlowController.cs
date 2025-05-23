using System;
using System.Collections.Generic;
using UnityEngine;

public class S_GameFlowController : MonoBehaviour
{
    public static S_GameFlowController Instance { get; private set; }
    public List<MonoBehaviour> stateComponents;
    public Type CurrentStateType { get; private set; }
    private S_IGameState _currentState;
    private Dictionary<Type, S_IGameState> _states = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        RegisterStates();
    }

    private void RegisterStates()
    {
        _states.Clear();

        foreach (var comp in stateComponents)
        {
            if (comp is S_IGameState state)
            {
                Type t = comp.GetType();
                _states[t] = state;
            }
        }
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
            case S_GameEvent.LoadLevel:
            case S_GameEvent.Restart:
                ChangeState<S_LoadingState>(args);
                break;
            case S_GameEvent.EnterGameState:
                ChangeState<S_InGameState>();
                break;
            case S_GameEvent.PauseGame:
                ChangeState<S_GamePauseState>();
                break;
            case S_GameEvent.GameOver:
                ChangeState<S_GameOverState>(args);
                break;
        }
    }

    private void ChangeState<T>(params object[] args) where T : S_IGameState
    {
        _currentState?.OnExit();

        if (_states.TryGetValue(typeof(T), out var nextState))
        {
            _currentState = nextState;
            CurrentStateType = typeof(T);
            _currentState.OnEnter(args); 
        }
    }
}
