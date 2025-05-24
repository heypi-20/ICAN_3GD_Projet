using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using FMOD.Studio;
using FMODUnity;

/// <summary>
/// Manages the Settings menu UI, including volume, music volume,
/// SFX volume, mouse sensitivity, screen resolution,
/// and toggling a special UI element. Settings are loaded
/// when the menu is opened and saved when it is closed.
/// </summary>
public class S_SettingsMenuBehavior : MonoBehaviour
{
    [Header("UI References")]    
    public Slider volumeSlider;                  // Slider for global volume
    public Slider musicVolumeSlider;             // Slider for music volume
    public Slider sfxVolumeSlider;               // Slider for SFX volume
    public Slider mouseSensitivitySlider;        // Slider for camera sensitivity
    public TMP_Dropdown resolutionDropdown;      // TextMeshPro dropdown for resolution
    public Toggle specialObjectToggle;           // Toggle for enabling/disabling a special object

    [Header("Panel Reference")]    
    public GameObject settingsPanel;             // Root GameObject for the settings menu

    private CinemachineVirtualCamera cinemachineCamera;  // Reference to the virtual camera

    private Resolution[] availableResolutions;   // All supported screen resolutions
    private S_HUDPlayerState infotext;           // Reference to a HUD element to toggle

    // Keys for PlayerPrefs storage
    private const string VolumeKey = "GlobalVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string SensitivityKey = "MouseSensitivity";
    private const string ResolutionKey = "ResolutionIndex";
    private const string ObjectToggleKey = "SpecialObjectEnabled";

    void Awake()
    {
        // Populate the resolution dropdown with all supported resolutions
        var filteredResolutions = Screen.resolutions
            .Select(res => new { res.width, res.height })
            .Distinct()
            .Select(res => new Resolution { width = res.width, height = res.height })
            .ToArray();

        availableResolutions = filteredResolutions;

        List<string> options = new List<string>();
        foreach (var res in availableResolutions)
            options.Add(res.width + " x " + res.height);

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);

        // Register UI event callbacks
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        mouseSensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        specialObjectToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnEnable()
    {
        // Cache references when the menu opens
        var gameSettings = FindObjectOfType<S_GameSettingsApplier>();
        if (gameSettings != null)
        {
            infotext = gameSettings.hud;
        }
        
        // Can't find it because object might be inactive
        if (cinemachineCamera == null)
            cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();

        // Load saved settings into the UI
        LoadSettings();
    }

    void OnDisable()
    {
        // Save settings when the menu closes
        SaveSettings();
        PlayerPrefs.Save();
    }

    void Update()
    {
        // Close settings menu on Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseSettings();
    }

    /// <summary>
    /// Closes the settings menu panel.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    /// <summary>
    /// Callback for a UI button to close the menu.
    /// </summary>
    public void OnCloseButton() => CloseSettings();

    /// <summary>
    /// Loads settings from PlayerPrefs into the UI and applies them.
    /// </summary>
    private void LoadSettings()
    {
        // Global volume
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeSlider.value = volume;
        RuntimeManager.GetBus("bus:/").setVolume(volume);

        // Music volume (FMOD bus)
        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        musicVolumeSlider.value = musicVol;
        RuntimeManager.GetBus("bus:/Music").setVolume(musicVol);

        // SFX volume (FMOD bus)
        float sfxVol = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
        sfxVolumeSlider.value = sfxVol;
        RuntimeManager.GetBus("bus:/SoundEffect").setVolume(sfxVol);

        // Mouse sensitivity (Cinemachine POV)
        float sens = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        mouseSensitivitySlider.value = sens;
        ApplyCinemachineSensitivity(sens);

        // Screen resolution
        int idx = PlayerPrefs.GetInt(ResolutionKey, availableResolutions.Length - 1);
        idx = Mathf.Clamp(idx, 0, availableResolutions.Length - 1);
        resolutionDropdown.value = idx;
        ApplyResolution(idx);

        // Special object toggle
        bool enabledObj = PlayerPrefs.GetInt(ObjectToggleKey, 1) == 1;
        specialObjectToggle.isOn = enabledObj;
        if (infotext != null)
            infotext.gameObject.SetActive(enabledObj);
    }

    /// <summary>
    /// Saves current UI values to PlayerPrefs.
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(VolumeKey, volumeSlider.value);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolumeSlider.value);
        PlayerPrefs.SetFloat(SFXVolumeKey, sfxVolumeSlider.value);
        PlayerPrefs.SetFloat(SensitivityKey, mouseSensitivitySlider.value);
        PlayerPrefs.SetInt(ResolutionKey, resolutionDropdown.value);
        PlayerPrefs.SetInt(ObjectToggleKey, specialObjectToggle.isOn ? 1 : 0);
    }

    /// <summary>
    /// Handler for global volume slider changes. Updates audio volume and PlayerPrefs.
    /// </summary>
    private void OnVolumeChanged(float val)
    {
        RuntimeManager.GetBus("bus:/").setVolume(val);
        PlayerPrefs.SetFloat(VolumeKey, val);

    }

    /// <summary>
    /// Handler for music volume slider changes. Updates FMOD music bus and PlayerPrefs.
    /// </summary>
    private void OnMusicVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, val);
        RuntimeManager.GetBus("bus:/Music").setVolume(val);
    }

    /// <summary>
    /// Handler for SFX volume slider changes. Updates FMOD SFX bus and PlayerPrefs.
    /// </summary>
    private void OnSFXVolumeChanged(float val)
    {
        PlayerPrefs.SetFloat(SFXVolumeKey, val);
        RuntimeManager.GetBus("bus:/SoundEffect").setVolume(val);
    }

    /// <summary>
    /// Handler for sensitivity slider changes. Updates Cinemachine POV and PlayerPrefs.
    /// </summary>
    private void OnSensitivityChanged(float val)
    {
        PlayerPrefs.SetFloat(SensitivityKey, val);
        ApplyCinemachineSensitivity(val);
    }

    /// <summary>
    /// Handler for resolution dropdown changes. Applies resolution and updates PlayerPrefs.
    /// </summary>
    private void OnResolutionChanged(int val)
    {
        ApplyResolution(val);
        PlayerPrefs.SetInt(ResolutionKey, val);
    }

    /// <summary>
    /// Handler for toggle changes. Updates HUD object and PlayerPrefs.
    /// </summary>
    private void OnToggleChanged(bool on)
    {
        PlayerPrefs.SetInt(ObjectToggleKey, on ? 1 : 0);
        if (infotext != null)
            infotext.gameObject.SetActive(on);
    }

    /// <summary>
    /// Applies the selected screen resolution.
    /// </summary>
    private void ApplyResolution(int idx)
    {
        var r = availableResolutions[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    /// <summary>
    /// Applies the given sensitivity to the Cinemachine POV component.
    /// </summary>
    private void ApplyCinemachineSensitivity(float sensitivity)
    {
        if (cinemachineCamera == null) return;
        var pov = cinemachineCamera.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            pov.m_HorizontalAxis.m_MaxSpeed = sensitivity;
            pov.m_VerticalAxis.m_MaxSpeed = sensitivity;
        }
    }
}
