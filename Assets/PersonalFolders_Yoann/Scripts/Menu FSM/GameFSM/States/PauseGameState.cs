using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameState : GameState
{
    public GameObject pauseMenuPanel;
    // private ChunkLoader chunkLoader;
    
    public override void Enter()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }
    

    public void Resume()
    {
        fsm.ChangeState(GetComponent<PlayingGameState>());
    }

    public void ToMenu()
    {
        // SceneManager.UnloadSceneAsync(chunkLoader.chunkPrefab.GetComponent<ChunkLoader>().chunkLoaded.BuildIndex);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        fsm.ChangeState(GetComponent<ReturnToMenuGameState>());
        
        //ToDo : Go To Main Menu
        Debug.Log("Main Menu");
    }
    
    public override void Exit()
    {
        pauseMenuPanel.SetActive(false);
        //Mettre le jeu en pause = responsabilité de PauseState.
        Time.timeScale = 1f; 
    }
}