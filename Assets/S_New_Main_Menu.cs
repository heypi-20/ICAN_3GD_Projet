using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Eflatun.SceneReference;

public class S_New_Main_Menu : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject myButton;
    public Transform buttonContainer;

    [Header("Scene List")]
    public SceneReference[] level;
    
    [Header("Checker")]
    // Variable pour stocker les panel principaux
    private GameObject currentMainPanel;
    
    // Variable pour stocker le panel actuellement ouvert
    private GameObject currentOpenSettingsPanel;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Crée un bouton pour chaque élément présent dans la liste
        foreach (var scenes in level)
        {
            CreateButton(scenes.Name);
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

    void OnButtonClicked(string buttonName)
    {
        Debug.Log("Lancement de la scène : " + buttonName);
        SceneManager.LoadScene(buttonName);
    }
    
    public void OpenMainPanel(GameObject panel)
    {
        // Ferme le panel principal actuel
        if (currentMainPanel != null && currentMainPanel != panel)
        {
            currentMainPanel.SetActive(false);
        }

        // Active le nouveau panel principal
        panel.SetActive(true);
        currentMainPanel = panel;
    }
    
    public void OpenSettingsPanel(GameObject panel)
    {
        // Vérifie si le panel de catégorie est différent du panel actuel
        if (currentOpenSettingsPanel != null && currentOpenSettingsPanel != panel)
        {
            currentOpenSettingsPanel.SetActive(false);
        }

        // Active le nouveau panel de catégorie
        panel.SetActive(true);
        currentOpenSettingsPanel = panel;
    }
    
    public void ClosePanel(GameObject panel)
    {
        // Désactive le panel que l'on passe en paramètre
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    public void Quit()
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}
