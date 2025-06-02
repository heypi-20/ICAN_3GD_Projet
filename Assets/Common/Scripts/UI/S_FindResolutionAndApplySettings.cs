using UnityEngine;
using Cinemachine;
using FMODUnity;

/// <summary>
/// Applies all player‑saved settings (resolution, volumes, mouse sensitivity
/// and special HUD toggle) on game start.
/// </summary>
public class S_FindResolutionAndApplySettings : MonoBehaviour
{
    // PlayerPrefs keys — keep identical to S_SettingsMenuBehavior.
    private const string VolumeKey        = "GlobalVolume";
    private const string MusicVolumeKey   = "MusicVolume";
    private const string SFXVolumeKey     = "SFXVolume";
    private const string SensitivityKey   = "MouseSensitivity";
    private const string ResolutionKey    = "ResolutionIndex";
    private const string ObjectToggleKey  = "SpecialObjectEnabled";

    void Start()
    {
        ApplySavedSettings();
    }

    /// <summary>
    /// Reads all known PlayerPrefs keys and applies them to the game
    /// before the first frame, so the player’s choices persist between sessions.
    /// </summary>
    private void ApplySavedSettings()
    {
        Resolution[] resolutions = Screen.resolutions;

        // 1. Screen resolution & fullscreen state.
        if (PlayerPrefs.HasKey(ResolutionKey))
        {
            int idx = Mathf.Clamp(PlayerPrefs.GetInt(ResolutionKey), 0, resolutions.Length - 1);
            var r = resolutions[idx];
            Screen.SetResolution(r.width, r.height, true);
        }
        else // First launch fallback (save current desktop res).
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, true);
            SaveCurrentResolutionAsDefault(resolutions, current);
        }

        // 2. Master, music and SFX volumes (FMOD buses).
        RuntimeManager.GetBus("bus:/").setVolume(PlayerPrefs.GetFloat(VolumeKey, 1f));
        RuntimeManager.GetBus("bus:/Music").setVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        RuntimeManager.GetBus("bus:/SoundEffect").setVolume(PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
    }

    /// <summary>
    /// Saves the current resolution as default on first launch.
    /// </summary>
    private void SaveCurrentResolutionAsDefault(Resolution[] all, Resolution current)
    {
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].width == current.width && all[i].height == current.height)
            {
                PlayerPrefs.SetInt(ResolutionKey, i);
                PlayerPrefs.Save();
                break;
            }
        }
    }
}
