using UnityEngine;
using Cinemachine;
using FMODUnity;

/// <summary>
/// Applies all player‑saved settings (resolution, volumes, mouse sensitivity
/// and special HUD toggle) on game start.
/// </summary>
public class S_FindResolutionAndApplySettings : MonoBehaviour
{
    private const string MasterKey     = "GlobalVolume";
    private const string MusicKey      = "MusicVolume";
    private const string SfxKey        = "SFXVolume";

    void Start()
    {
        ApplyDefaultResolutionIfFirstTime();
        ApplyVolumeFromPrefs();
    }
    private void ApplyDefaultResolutionIfFirstTime()
    {
        if (!PlayerPrefs.HasKey("ResolutionIndex"))
        {
            Resolution current = Screen.currentResolution;

            Screen.SetResolution(current.width, current.height, true);

            Resolution[] all = Screen.resolutions;
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].width == current.width && all[i].height == current.height)
                {
                    PlayerPrefs.SetInt("ResolutionIndex", i);
                    PlayerPrefs.Save();
                    break;
                }
            }
        }
    }
    void ApplyVolumeFromPrefs()
    {
        RuntimeManager.GetBus("bus:/")            .setVolume(PlayerPrefs.GetFloat(MasterKey, 1f));
        RuntimeManager.GetBus("bus:/Music")       .setVolume(PlayerPrefs.GetFloat(MusicKey,  1f));
        RuntimeManager.GetBus("bus:/SoundEffect") .setVolume(PlayerPrefs.GetFloat(SfxKey,    1f));
    }
}
