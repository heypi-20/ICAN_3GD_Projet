using System;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class S_CameraSprintFeedback : MonoBehaviour
{
    #region Inspector
    [Header("FOV Settings")]
    public float FOV_Pallier2 = 65f;
    public float FOV_Pallier3 = 75f;
    public float FOV_Pallier4 = 90f;
    public float fovTransitionTime = 0.3f;

    [Header("Sprint Hit Feedback")]
    public float baseDutch  = 2f;
    public float maxDutch   = 8f;
    public float dutchDuration = 0.2f;
    public AnimationCurve dutchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lens Distortion Settings")]
    public float Distortion_Pallier2 = -0.2f;
    public float Distortion_Pallier3 = -0.3f;
    public float Distortion_Pallier4 = -0.4f;
    public float distortionDuration  = 0.3f;
    #endregion

    // Shared FOV tween Id so that only one FOV tween exists scene-wide
    private const string FOV_TWEEN_ID   = "FOV";
    private const string DIST_TWEEN_ID  = "SPRINT_DISTORT";
    private const string DUTCH_TWEEN_ID = "SPRINT_DUTCH";

    private LensDistortion _lensDistortion;
    private Tween _distortionTween;

    private CinemachineVirtualCamera _vcam;
    private float _preSprintFOV;   // Live FOV captured when sprint starts
    private float _currentDutch;
    private Tween _dutchTween;
    private bool  _dutchDirectionRight = true;

    #region Setup
    public void Setup(CinemachineVirtualCamera vcam)
    {
        _vcam = vcam;

        var volumeSettings = _vcam.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings != null &&
            volumeSettings.m_Profile != null &&
            volumeSettings.m_Profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;
        }
        else
        {
            Debug.LogWarning("[S_CameraSprintFeedback] LensDistortion not found in volume profile!");
        }
    }
    #endregion

    #region Public Event
    public void ReceiveSprintEvent(Enum state, int sprintLevel)
    {
        if (state.Equals(PlayerStates.SprintState.IsSprinting))
        {
            // Capture FOV before sprint modifies it
            _preSprintFOV = _vcam.m_Lens.FieldOfView;

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
    #endregion

    #region Distortion
    private void StartSprintDistortion(int level)
    {
        if (_lensDistortion == null) return;

        float target = level switch
        {
            2 => Distortion_Pallier2,
            3 => Distortion_Pallier3,
            4 => Distortion_Pallier4,
            _ => 0f
        };

        _distortionTween?.Kill();
        _distortionTween = DOTween.To(() => _lensDistortion.intensity.value,
                                      x => _lensDistortion.intensity.value = x,
                                      target,
                                      distortionDuration)
                                  .SetEase(Ease.OutSine)
                                  .SetId(DIST_TWEEN_ID);
    }

    private void StopSprintDistortion()
    {
        if (_lensDistortion == null) return;

        _distortionTween?.Kill();
        _distortionTween = DOTween.To(() => _lensDistortion.intensity.value,
                                      x => _lensDistortion.intensity.value = x,
                                      0f,
                                      distortionDuration)
                                  .SetEase(Ease.InSine)
                                  .SetId(DIST_TWEEN_ID);
    }
    #endregion

    #region FOV
    private void StartSprintFOV(int level)
    {
        float targetFOV = level switch
        {
            2 => FOV_Pallier2,
            3 => FOV_Pallier3,
            4 => FOV_Pallier4,
            _ => _vcam.m_Lens.FieldOfView
        };

        DOTween.Kill(FOV_TWEEN_ID, complete: false);

        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                   x  => _vcam.m_Lens.FieldOfView = x,
                   targetFOV,
                   fovTransitionTime)
               .SetEase(Ease.OutSine)
               .SetId(FOV_TWEEN_ID);
    }

    private void StopSprintFOV()
    {
        DOTween.Kill(FOV_TWEEN_ID, complete: false);

        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                   x  => _vcam.m_Lens.FieldOfView = x,
                   _preSprintFOV,
                   fovTransitionTime)
               .SetEase(Ease.InSine)
               .SetId(FOV_TWEEN_ID);
    }
    #endregion

    #region Dutch (Sprint Hit)
    private void ApplySprintHitFeedback(int hitCount)
    {
        float strength = Mathf.Min(baseDutch * hitCount, maxDutch);
        float targetDutch = _dutchDirectionRight ? strength : -strength;
        _dutchDirectionRight = !_dutchDirectionRight; // Alternate direction

        _dutchTween?.Kill();

        _dutchTween = DOTween.Sequence()
            .Append(DOTween.To(() => _vcam.m_Lens.Dutch,
                               x => _vcam.m_Lens.Dutch = x,
                               targetDutch,
                               dutchDuration / 2f)
                     .SetEase(dutchCurve))
            .Append(DOTween.To(() => _vcam.m_Lens.Dutch,
                               x => _vcam.m_Lens.Dutch = x,
                               0f,
                               dutchDuration / 2f)
                     .SetEase(dutchCurve))
            .SetId(DUTCH_TWEEN_ID);
    }
    #endregion
}
