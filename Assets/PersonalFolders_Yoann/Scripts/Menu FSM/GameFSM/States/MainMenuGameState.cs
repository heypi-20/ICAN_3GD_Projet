using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Eflatun.SceneReference;

public class MainMenuGameState : GameState
{
    public GameObject menuGO;
    
    [Header("Prefab")]
    public GameObject myButton;
    public Transform buttonContainer;

    [Header("Scene List")]
    public SceneReference[] level;
    
    [Header("Checker")]
    // Variable pour stocker les panel principaux
    private GameObject currentMainPanel;
    
    void OnButtonClicked(string buttonName)
    {
        Debug.Log("Lancement de la scène : " + buttonName);
        SceneManager.LoadScene(buttonName);
        
        //fsm.selectedLevel = buttonName;
        fsm.ChangeState(GetComponent<LoadingLevelGameState>());
    }
    
    void CreateButton(string buttonName)
    {
        // Instancie le bouton et configure son texte
        GameObject newButton = Instantiate(myButton, buttonContainer);
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = buttonName;
        }

        Button buttonComponent = newButton.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => OnButtonClicked(buttonName));
        }
    }
    
    public override void Enter()
    {
        menuGO.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Crée un bouton pour chaque élément présent dans la liste
        foreach (var scenes in level)
        {
            CreateButton(scenes.Name);
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