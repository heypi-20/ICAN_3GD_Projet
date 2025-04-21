using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using TMPro;

public class S_CameraSensitivitySlider : MonoBehaviour
{
    public CinemachineVirtualCamera cam;         // Reference to the Cinemachine Virtual Camera
    public Slider sensitivitySlider;             // Slider UI element
    public TextMeshProUGUI sensitivityText;      // Text UI to display sensitivity value

    public float sensitivityMultiplier = 10f;    // Multiplier to scale slider value

    void Start()
    {
        if (cam != null && sensitivitySlider != null)
        {
            var pov = cam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                float currentSensitivity = pov.m_HorizontalAxis.m_MaxSpeed / sensitivityMultiplier;
                sensitivitySlider.value = currentSensitivity;
                UpdateSensitivityText(currentSensitivity);
            }
        }
    }

    // Called when the slider value changes
    public void OnSliderValueChanged(float value)
    {
        var pov = cam.GetCinemachineComponent<CinemachinePOV>();
        if (pov != null)
        {
            float newSensitivity = value * sensitivityMultiplier;

            pov.m_HorizontalAxis.m_MaxSpeed = newSensitivity;
            pov.m_VerticalAxis.m_MaxSpeed = newSensitivity;

            UpdateSensitivityText(value);
        }
    }

    // Updates the UI text to show the current sensitivity
    void UpdateSensitivityText(float sliderValue)
    {
        if (sensitivityText != null)
        {
            float displayValue = sliderValue;
            sensitivityText.text = displayValue.ToString("F1"); // Show 1 decimal place
        }
    }
}