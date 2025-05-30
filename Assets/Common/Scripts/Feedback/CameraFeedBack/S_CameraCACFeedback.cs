using System;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class S_CameraCACFeedback : MonoBehaviour
{
    #region Inspector
    [Header("Shake Settings")]
    public float shakeLvl1 = 1f;
    public float shakeLvl2 = 2f;
    public float shakeLvl3 = 3f;
    public float shakeLvl4 = 4f;
    public float weaknessMultiplier = 1.5f;

    [Header("FOV Settings")]
    public float fovMultiplier = 20f;
    public float fovDuration  = 0.4f;
    public float maxFOV       = 130f;

    [Header("Lens Distortion Settings")]
    public float distortionMax     = -0.4f;
    public float distortionDuration = 0.5f;
    public float distortionLimit    = -0.5f;
    #endregion

    private const string FOV_TWEEN_ID   = "FOV";         // Shared across all camera scripts
    private const string DIST_TWEEN_ID  = "CAC_DISTORT"; // Local to this script

    private CinemachineVirtualCamera _vcam;
    private CinemachineImpulseSource _impulseSource;
    private LensDistortion _lensDistortion;

    private bool  _isIncreasing;
    private float _baseFOV;
    private float _startFOV;
    private float _timePassed;
    
    

    #region Public API
    public void Setup(CinemachineVirtualCamera vcam, CinemachineImpulseSource impulse)
    {
        _vcam          = vcam;
        _impulseSource = impulse;
        _baseFOV  = _vcam.m_Lens.FieldOfView;
        _startFOV = _baseFOV;

        // Grab Lens Distortion from the camera’s volume profile (if any)
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
        // Early-out if FOV isn’t growing
        if (!_isIncreasing) return;

        // Stop growing once max FOV is reached and clamp to limit
        if (_vcam.m_Lens.FieldOfView >= maxFOV)
        {
            _vcam.m_Lens.FieldOfView = maxFOV;
            _isIncreasing = false;
            return;
        }

        // Accumulate FOV & Distortion over time (physics step)
        _timePassed += Time.fixedDeltaTime;

        float newFOV = _startFOV + _timePassed * fovMultiplier;
        _vcam.m_Lens.FieldOfView = Mathf.Min(newFOV, maxFOV);

        if (_lensDistortion != null)
        {
            float target = _timePassed * distortionMax;
            _lensDistortion.intensity.value = Mathf.Max(target, distortionLimit);
        }
    }

    public void ReceiveMeleeAttackEvent(Enum state, int level)
    {
        // Dash start: begin increasing FOV
        if (state.Equals(PlayerStates.MeleeState.DashingBeforeMelee))
        {
            _startFOV    = 60f; // Use live FOV as baseline
            _timePassed  = 0f;
            _isIncreasing = true;

            // Ensure no other FOV tweens are running
            DOTween.Kill(FOV_TWEEN_ID, complete: false);
            return;
        }

        // Hit (normal or weakness)
        if (state.Equals(PlayerStates.MeleeState.MeleeAttackHit) ||
            state.Equals(PlayerStates.MeleeState.MeleeAttackHitWeakness))
        {
            _isIncreasing = false;

            float intensity = GetShakeLevel(level);
            if (state.Equals(PlayerStates.MeleeState.MeleeAttackHitWeakness))
                intensity *= weaknessMultiplier;

            TriggerShake(intensity);
            ResetDistortion();
            ResetFOV();
        }
    }

    #region Helpers
    private float GetShakeLevel(int level) => level switch
    {
        1 => shakeLvl1,
        2 => shakeLvl2,
        3 => shakeLvl3,
        4 => shakeLvl4,
        _ => 1f
    };

    private void TriggerShake(float intensity)
    {
        _impulseSource?.GenerateImpulse(Vector3.one * intensity);
    }

    private void ResetDistortion()
    {
        if (_lensDistortion == null) return;

        // Kill any previous distortion tween before starting a new one
        DOTween.Kill(DIST_TWEEN_ID, complete: false);

        DOTween.To(() => _lensDistortion.intensity.value,
                   x  => _lensDistortion.intensity.value = x,
                   0f,
                   distortionDuration)
               .SetEase(Ease.OutElastic)
               .SetId(DIST_TWEEN_ID);
    }

    private void ResetFOV()
    {
        // Kill any existing FOV tween so we own the property exclusively
        DOTween.Kill(FOV_TWEEN_ID, complete: false);

        DOTween.To(() => _vcam.m_Lens.FieldOfView,
                x  => _vcam.m_Lens.FieldOfView = x,
                _baseFOV,            
                fovDuration)
            .SetEase(Ease.OutSine)
            .SetId(FOV_TWEEN_ID);
    }
    #endregion
}
