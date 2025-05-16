using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class S_CameraSprintFeedback : MonoBehaviour
{
    [Header("FOV Settings")]
    public float FOV_Pallier2 = 65f;
    public float FOV_Pallier3 = 75f;
    public float FOV_Pallier4 = 90f;
    public float fovTransitionTime = 0.3f;

    [Header("Sprint Hit Feedback")]
    public float baseDutch = 2f;
    public float maxDutch = 8f;
    public float dutchDuration = 0.2f;
    public AnimationCurve dutchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lens Distortion Settings")] 
    public float Distortion_Pallier2 = -0.2f;
    public float Distortion_Pallier3 = -0.3f;
    public float Distortion_Pallier4 = -0.4f;
    public float distortionDuration = 0.3f;

    private LensDistortion _lensDistortion;
    private Tween _distortionTween;

    private CinemachineVirtualCamera _vcam;
    private CinemachineImpulseSource _impulse;
    private float _originalFOV;
    private float _currentDutch;
    private Coroutine _dutchCoroutine;
    private Tween _dutchTween;
    private bool _dutchDirectionRight = true;

    public void Setup(CinemachineVirtualCamera vcam)
    {
        _vcam = vcam;
        _originalFOV = _vcam.m_Lens.FieldOfView;

        var volumeSettings = _vcam.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings != null && volumeSettings.m_Profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("❌ LensDistortion not found in volume profile!");
        }
    }

    public void ReceiveSprintEvent(Enum state, int sprintLevel)
    {
        if (state.Equals(PlayerStates.SprintState.IsSprinting))
        {
            StartSprintFOV(sprintLevel);
            StartSprintDistortion(sprintLevel);
        }
        else if (state.Equals(PlayerStates.SprintState.StopSprinting))
        {
            StopSprintFOV();
            StopSprintDistortion();
        }
        else if (state.Equals(PlayerStates.SprintState.SprintHit))
        {
            ApplySprintHitFeedback(sprintLevel);
        }
    }
    
    private void StartSprintDistortion(int level)
    {
        if (_lensDistortion == null) return;

        float targetDistortion = 0f;

        switch (level)
        {
            case 2:
                targetDistortion = Distortion_Pallier2;
                break;
            case 3:
                targetDistortion = Distortion_Pallier3;
                break;
            case 4:
                targetDistortion = Distortion_Pallier4;
                break;
        }

        _distortionTween?.Kill();
        _distortionTween = DOTween.To(() => _lensDistortion.intensity.value,
                x => _lensDistortion.intensity.value = x,
                targetDistortion,
                distortionDuration)
            .SetEase(Ease.OutSine);
    }
    private void StopSprintDistortion()
    {
        if (_lensDistortion == null) return;

        _distortionTween?.Kill();
        _distortionTween = DOTween.To(() => _lensDistortion.intensity.value,
                x => _lensDistortion.intensity.value = x,
                0f,
                distortionDuration)
            .SetEase(Ease.InSine);
    }
    
    private void StartSprintFOV(int level)
    {
        float targetFOV = level switch
        {
            2 => FOV_Pallier2,
            3 => FOV_Pallier3,
            4 => FOV_Pallier4
        };
        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                   x => _vcam.m_Lens.FieldOfView = x,
                   targetFOV,
                   fovTransitionTime)
               .SetEase(Ease.OutSine);
    }

    private void StopSprintFOV()
    {
        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                   x => _vcam.m_Lens.FieldOfView = x,
                   _originalFOV,
                   fovTransitionTime)
               .SetEase(Ease.InSine);
    }

    private void ApplySprintHitFeedback(int hitCount)
    {
        float strength = Mathf.Min(baseDutch * hitCount, maxDutch);
        float targetDutch = _dutchDirectionRight ? strength : -strength;

        _dutchDirectionRight = !_dutchDirectionRight; // Alterner la direction

        _dutchTween?.Kill();

        _dutchTween = DOTween.To(
                () => _vcam.m_Lens.Dutch,
                x => _vcam.m_Lens.Dutch = x,
                targetDutch,
                dutchDuration / 2f
            )
            .SetEase(dutchCurve)
            .OnComplete(() =>
            {
                DOTween.To(() => _vcam.m_Lens.Dutch,
                        x => _vcam.m_Lens.Dutch = x,
                        0f,
                        dutchDuration / 2f)
                    .SetEase(dutchCurve);
            });
    }

}
