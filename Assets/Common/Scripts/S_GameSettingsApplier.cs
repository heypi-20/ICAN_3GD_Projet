using UnityEngine;
using Cinemachine;

public class S_GameSettingsApplier : MonoBehaviour
{
    void Start()
    {
        ApplyVolume();
        ApplySensitivity();
        ApplyResolution();
        ApplyToggle();
    }

    private void ApplyVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("GlobalVolume", 1f);
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

    private void ApplyResolution()
    {
        var resolutions = Screen.resolutions;
        int idx = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        idx = Mathf.Clamp(idx, 0, resolutions.Length - 1);
        var r = resolutions[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    private void ApplyToggle()
    {
        bool enabledObj = PlayerPrefs.GetInt("SpecialObjectEnabled", 1) == 1;
        var hud = FindObjectOfType<S_HUDPlayerState>();
        if (hud != null)
            hud.gameObject.SetActive(enabledObj);
    }
}
