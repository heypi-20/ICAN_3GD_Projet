using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Eflatun.SceneReference;

public class S_MenuLibrary : MonoBehaviour
{
    [Header("Checker")]
    // Variable pour stocker les panel principaux
    private GameObject currentMainPanel;
    
    // Variable pour stocker le panel actuellement ouvert
    private GameObject currentOpenSettingsPanel;
    
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
    
    public void Quit()
    {
        Debug.Log("Closing Game");
        Application.Quit();
    }
}
