using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // Pour des requetes SQL en general

public class S_SettingMenu : MonoBehaviour
{
    Resolution[] resolutions;

    [Header("Dropdown")]
    public TMP_Dropdown resolutionDropdown;

    void Start()
    {
        ResolutionInDropdown();
    }

    public void SetVolume(float volume)
    {
        Debug.Log(volume);
    }

    public void SetFullScreen(bool isFullScreen)
    {
        Debug.Log("FullScreen = " + isFullScreen);
        Screen.fullScreen = isFullScreen;
        
    }

    void ResolutionInDropdown()
    {
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height}).Distinct().ToArray(); // Permet de stocker toutes les resolution existante sur le PC et d'eviter les doublon grace a "Select" de Linq
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + "x" + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue(); // Permet d'avoir la bonne valeur affiché dans le Dropdown
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log("La resolution actuel est : " + resolution.width + "x" + resolution.height);
    }

}
