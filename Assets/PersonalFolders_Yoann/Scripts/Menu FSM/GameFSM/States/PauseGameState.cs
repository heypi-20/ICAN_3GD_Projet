using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;


public class PauseGameState : GameState
{
    public GameObject pauseMenuPanel;
    
    public override void Enter()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        
        GameObject player = GameObject.Find("PreDenis_PlayerObjGroupe_Variation");
        player.GetComponentInChildren<Canvas>().enabled = false;
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
        GameObject playerCanvas = GameObject.Find("Player Canvas");
        playerCanvas.GetComponent<Canvas>().enabled = true;
    }
}