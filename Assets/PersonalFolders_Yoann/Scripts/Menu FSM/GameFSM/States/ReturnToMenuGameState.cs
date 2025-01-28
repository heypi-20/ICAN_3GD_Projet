using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuGameState : GameState
{
    public GameObject loadingMenu;
    
    [SerializeField] private float minLoadingTime = 0.5f;
    
    private AsyncOperation asyncLoad;
    public SceneReference menuScene;
    private float minTransitionTime;
    
    public override void Enter()
    {
        minTransitionTime = Time.time + minLoadingTime;
        loadingMenu.SetActive(true);
        asyncLoad = SceneManager.LoadSceneAsync(menuScene.BuildIndex);
    }
    
    public override void Tick()
    {
        if (asyncLoad.isDone && Time.time >= minTransitionTime)
        {
            fsm.ChangeState(GetComponent<MainMenuGameState>());
        }
    }

    public override void Exit()
    {
        loadingMenu.SetActive(false);
    }
    
}