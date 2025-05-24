using UnityEngine;
using Cinemachine;
using FMODUnity;

public class S_GameSettingsApplier : MonoBehaviour
{
    public S_HUDPlayerState hud;

    void Start()
    {
        ApplyGlobalVolume();
        ApplyMusicVolume();
        ApplySFXVolume();
        ApplySensitivity();
        ApplyToggle();
    }

    private void ApplyGlobalVolume()
    {
        RuntimeManager.GetBus("bus:/").setVolume(PlayerPrefs.GetFloat("GlobalVolume", 1f));
    }

    private void ApplyMusicVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        RuntimeManager.GetBus("bus:/Music").setVolume(musicVol);
    }

    private void ApplySFXVolume()
    {
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        RuntimeManager.GetBus("bus:/SoundEffect").setVolume(sfxVol);
    }

    private void ApplySensitivity()
    {
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                float sens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
                pov.m_HorizontalAxis.m_MaxSpeed = sens;
                pov.m_VerticalAxis.m_MaxSpeed = sens;
            }
        }
    }

    private void ApplyToggle()
    {
        bool enabledObj = PlayerPrefs.GetInt("SpecialObjectEnabled", 1) == 1;
        if (hud != null)
            hud.gameObject.SetActive(enabledObj);
    }
}