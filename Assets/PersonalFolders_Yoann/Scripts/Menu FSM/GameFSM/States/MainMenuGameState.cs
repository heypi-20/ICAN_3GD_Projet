using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Eflatun.SceneReference;
using UnityEngine.Serialization;

public class MainMenuGameState : GameState
{
    public GameObject menuGO;
    public GameObject levelPanel;
    
    [Header("Prefab")]
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    [Header("Scene List")]
    public SceneReference[] level;
    
    [Header("Checker")]
    // Variable pour stocker les panel principaux
    private GameObject currentMainPanel;
    
    void OnButtonClicked(SceneReference scene)
    {
        fsm.selectedLevel = scene;
        fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    }
    
    void CreateButton(SceneReference scene)
    {
        GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = scene.Name; // Utilise le nom de la scène
        }

        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() =>
            {
                levelPanel.SetActive(false); // Désactive le panneau de niveaux
                OnButtonClicked(scene);      // Appelle la méthode pour changer l'état
            });
        }
    }
    
    public override void Enter()
    {
        menuGO.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Crée un bouton pour chaque élément présent dans la liste
        foreach (var scene in level)
        {
            CreateButton(scene);
        }
    }

    public override void Tick()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Closing Game");
            Application.Quit();
        }
    }
    
    // //Used by button - single responsibility
    // public void TransitionToLevel1()
    // {
    //     fsm.selectedLevel = fsm.level1;
    //     fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    // }

    // public void TransitionToLevel2()
    // {
    //     fsm.selectedLevel = fsm.level2;
    //     fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    // }

    public override void Exit()
    {
        menuGO.SetActive(false);
    }
}