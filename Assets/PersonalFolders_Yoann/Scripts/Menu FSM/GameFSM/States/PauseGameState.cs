using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;


public class PauseGameState : GameState
{
    public GameObject pauseMenuPanel;
    private GameObject optionalDisable;

    public override void Enter()
    {

        optionalDisable = GameObject.FindGameObjectWithTag("OptionalDisable");
        if (optionalDisable != null)
        {
            optionalDisable.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Objet avec le tag 'MonTag' non trouvé !");
        }
        
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        
        GameObject player = GameObject.Find("PreDenis_PlayerObjGroupe_Variation");
        
        if (player != null)
        {
            player.GetComponentInChildren<Canvas>().enabled = false;
        }
        
    }

    public override void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Resume();
        }
    }

    public void Resume()
    {
        fsm.ChangeState(GetComponent<PlayingGameState>());
    }

    public void ToMenu()
    {
        // SceneManager.UnloadSceneAsync(chunkLoader.chunkPrefab.GetComponent<ChunkLoader>().chunkLoaded.BuildIndex);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        fsm.ChangeState(GetComponent<ReturnToMenuGameState >());
        
        //ToDo : Go To Main Menu
        Debug.Log("Main Menu");
    }
    
    public override void Exit()
    {
        pauseMenuPanel.SetActive(false);
        //Mettre le jeu en pause = responsabilité de PauseState.
        Time.timeScale = 1f; 
        GameObject playerCanvas = GameObject.Find("Player Canvas");
        
        if (playerCanvas != null)
        {
            playerCanvas.GetComponent<Canvas>().enabled = true;
        }
        
        if (optionalDisable != null)
        {
            optionalDisable.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Objet avec le tag 'MonTag' non trouvé !");
        }
    }
}