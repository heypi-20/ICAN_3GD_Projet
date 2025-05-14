using System;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX; 
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class S_CameraCACFeedback : MonoBehaviour
{
    [Header("Shake Settings")]
    public float shakeLvl1 = 1f;
    public float shakeLvl2 = 2f;
    public float shakeLvl3 = 3f;
    public float shakeLvl4 = 4f;
    public float weaknessMultiplier = 1.5f;

    [Header("FOV Settings")]
    public float fovMultiplier = 20f;
    public float fovDuration = 0.4f;
    public float maxFOV = 130f; 

    [Header("Lens Distortion Settings")]
    public float distortionMax = -0.4f;
    public float distortionDuration = 0.5f;
    public float distortionLimit = -0.5f;

    private CinemachineVirtualCamera _vcam;
    private CinemachineImpulseSource _impulseSource;
    private LensDistortion _lensDistortion;
    private bool _isIncreasing;
    private float _startFOV;
    private float _timePassed;

    public void Setup(CinemachineVirtualCamera vcam, CinemachineImpulseSource impulse)
    {
        _vcam = vcam;
        _impulseSource = impulse;

        var volumeSettings = _vcam.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings != null && volumeSettings.m_Profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!_isIncreasing) return;

        _timePassed += Time.deltaTime;
        float newFOV = _startFOV + _timePassed * fovMultiplier;
        _vcam.m_Lens.FieldOfView = Mathf.Min(newFOV, maxFOV);

        if (_lensDistortion != null)
        {
            float targetDistortion = _timePassed * distortionMax;
            _lensDistortion.intensity.value = Mathf.Max(targetDistortion, distortionLimit);
        }
    }

    public void ReceiveMeleeAttackEvent(Enum state, int level)
    {
        if (state.Equals(PlayerStates.MeleeState.DashingBeforeMelee))
        {
            _startFOV = _vcam.m_Lens.FieldOfView;
            _timePassed = 0f;
            _isIncreasing = true;
            return;
        }

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

    private float GetShakeLevel(int level)
    {
        return level switch
        {
            1 => shakeLvl1,
            2 => shakeLvl2,
            3 => shakeLvl3,
            4 => shakeLvl4,
            _ => 1f,
        };
    }

    private void TriggerShake(float intensity)
    {
        _impulseSource?.GenerateImpulse(Vector3.one * intensity);
    }

    private void ResetDistortion()
    {
        if (_lensDistortion == null) return;

        DOTween.To(() => _lensDistortion.intensity.value,
                   x => _lensDistortion.intensity.value = x,
                   0f,
                   distortionDuration)
               .SetEase(Ease.OutElastic);
    }

    private void ResetFOV()
    {
        float currentFOV = _vcam.m_Lens.FieldOfView;
        DOTween.To(() => currentFOV,
                   x => _vcam.m_Lens.FieldOfView = x,
                   _startFOV,
                   fovDuration)
               .SetEase(Ease.OutSine);
    }
}
