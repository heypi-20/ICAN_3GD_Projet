using System;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class S_CameraGroundPoundFeedback : MonoBehaviour
{
    #region Inspector
    [Header("Shake Settings")]
    public float startShakeForce = 3f;
    public float shakeMultiplier = 5f;

    [Header("FOV Settings")]
    public float fovMultiplier = 25f;
    public float fovDuration   = 0.5f;
    public float maxFOV        = 130f;

    [Header("Lens Distortion Settings")]
    public float distortionMax     = -0.5f;
    public float distortionDuration = 0.5f;
    public float distortionLimit    = -0.8f;

    [Header("Dutch Oscillation Settings")]
    public float dutchMaxAmplitude = 0.5f;
    public float dutchFrequency    = 5f;
    #endregion

    private const string FOV_TWEEN_ID  = "FOV";        // Shared across camera scripts
    private const string DIST_TWEEN_ID = "GP_DISTORT"; // Local to this script

    private CinemachineVirtualCamera _vcam;
    private CinemachineImpulseSource _impulseSource;
    private LensDistortion _lensDistortion;

    private float _startFOV;
    private float _timePassed;
    private bool  _isCharging;
    private float _accumulatedForce;

    #region Setup
    public void Setup(CinemachineVirtualCamera vcam, CinemachineImpulseSource impulse)
    {
        _vcam          = vcam;
        _impulseSource = impulse;
        _startFOV      = 60f;

        var volumeSettings = _vcam.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings != null &&
            volumeSettings.m_Profile != null &&
            volumeSettings.m_Profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;
        }
    }
    #endregion

    private void FixedUpdate()
    {
        if (!_isCharging) return;

        // Accumulate charge time
        _timePassed += Time.fixedDeltaTime;
        _accumulatedForce = startShakeForce + _timePassed * shakeMultiplier;

        // Dutch oscillation while charging
        if (_vcam != null)
        {
            float amplitude  = Mathf.Min(_timePassed * dutchMaxAmplitude, dutchMaxAmplitude);
            float dutchValue = Mathf.Sin(Time.time * dutchFrequency * Mathf.PI * 2f) * amplitude;
            _vcam.m_Lens.Dutch = dutchValue;
        }

        // Live FOV growth, clamped to max
        float targetFOV = _startFOV + _timePassed * fovMultiplier;
        _vcam.m_Lens.FieldOfView = Mathf.Min(targetFOV, maxFOV);

        // Lens distortion growth
        if (_lensDistortion != null)
        {
            float targetDist = _timePassed * distortionMax;
            _lensDistortion.intensity.value = Mathf.Max(targetDist, distortionLimit);
        }
    }

    public void ReceiveGroundPoundEvent(Enum state, int level)
    {
        if (state.Equals(PlayerStates.GroundPoundState.StartGroundPound))
        {
            // Reset charging state
            _isCharging       = true;
            _timePassed       = 0f;
            _accumulatedForce = 0f;

            // Use live FOV as baseline and kill any active FOV tween
            _startFOV = _vcam.m_Lens.FieldOfView;
            DOTween.Kill(FOV_TWEEN_ID, complete: false);
            return;
        }

        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            _isCharging = false;

            TriggerImpulse(_accumulatedForce);
            ResetDistortion();
            ResetFOV();
        }
    }

    #region Helpers
    private void TriggerImpulse(float force)
    {
        _impulseSource?.GenerateImpulse(Vector3.one * force);
    }

    private void ResetFOV()
    {
        DOTween.Kill(FOV_TWEEN_ID, complete: false);

        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                   x  => _vcam.m_Lens.FieldOfView = x,
                   _startFOV,
                   fovDuration)
               .SetEase(Ease.OutSine)
               .SetId(FOV_TWEEN_ID);

        // Immediately reset Dutch; could be tweened if desired
        _vcam.m_Lens.Dutch = 0f;
    }

    private void ResetDistortion()
    {
        if (_lensDistortion == null) return;

        DOTween.Kill(DIST_TWEEN_ID, complete: false);

        DOTween.To(() => _lensDistortion.intensity.value,
                   x  => _lensDistortion.intensity.value = x,
                   0f,
                   distortionDuration)
               .SetEase(Ease.OutElastic)
               .SetId(DIST_TWEEN_ID);
    }
    #endregion
}
