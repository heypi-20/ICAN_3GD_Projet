using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject myButton;
    public Transform buttonContainer;

    [Header("Scene List")]
    public string[] level;

    [Header("Menu")]
    public GameObject levelWindow;
    public GameObject settingWindow;

    void Start()
    {
        
        // Créer un bouton pour chaque éléments présent dans la liste
        foreach (string levelName in level)
        {
            CreateButton(levelName);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Closing Game");
            Application.Quit();
        }
    }

    void CreateButton(string buttonName)
    {
        // On instancie le Bouton qui est un GameObject et on le stock dans un variable pour la manipulé
        GameObject newButton = Instantiate(myButton, buttonContainer);

        // On récupere le component TextMeshProUGUI pour modifier le texte du bouton
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        
        if (buttonText != null) // Si l'élément de la liste n'est pas sans nom
        {
            buttonText.text = buttonName; // Le Texte devient la string de son index dans la liste
        }

        Button buttonComponent = newButton.GetComponent<Button>();
        
        if (buttonComponent != null)
        {
            buttonComponent.onClick.AddListener(() => OnButtonClicked(buttonName)); // Permet de relier OnButtonClicked 
        }
    }

    void OnButtonClicked(string buttonName)
    {
        Debug.Log("Lancement de la scene : " + buttonName);
        SceneManager.LoadScene(buttonName);
    }

    public void OpenLevelButton()
    {
        levelWindow.SetActive(true);
    }

    public void CloseLevelButton()
    {
        levelWindow.SetActive(false);
    }

    public void OpenSettingButton()
    {
        settingWindow.SetActive(true);
    }

    public void CloseSettingButton()
    {
        settingWindow.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}
