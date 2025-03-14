﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LoadingLevelGameState : GameState
{
    public GameObject loadingMenuGO;
    //Wait at least this amount of time :
    [SerializeField] private float minLoadingTime = 0.5f;
    
    private AsyncOperation asyncLoad;
    private float minTransitionTime;

    public bool avoidInitState = false;
    
    
    public override void Enter()
    {
        minTransitionTime = Time.time + minLoadingTime;
        loadingMenuGO.SetActive(true);
        asyncLoad = SceneManager.LoadSceneAsync(fsm.selectedLevel.BuildIndex, LoadSceneMode.Additive);
    }

    public override void Tick()
    {
        if (asyncLoad.isDone && Time.time >= minTransitionTime)
        {
            SceneManager.SetActiveScene(fsm.selectedLevel.LoadedScene);
            if (avoidInitState)
            {
                fsm.ChangeState(GetComponent<PlayingGameState>());
            }
            else
            {
                fsm.ChangeState(GetComponent<LevelInitializationGameState>());
            }
        }
        
        //todo : show loading bar/ image...
    }

    public override void Exit()
    {
        loadingMenuGO.SetActive(false);
    }
}