// S_CameraGroundPoundFeedback.cs
using System;
using System.Collections;
using UnityEngine;
using Cinemachine;
using Cinemachine.PostFX; 
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class S_CameraGroundPoundFeedback : MonoBehaviour
{
    [Header("Shake Settings")]
    public float startShakeForce = 3f;
    public float shakeMultiplier = 5f;

    [Header("FOV Settings")]
    public float fovMultiplier = 25f;
    public float fovDuration = 0.5f;
    public float maxFOV = 130f;

    [Header("Lens Distortion Settings")]
    public float distortionMax = -0.5f;
    public float distortionDuration = 0.5f;
    public float distortionLimit = -0.8f;
    
    [Header("Dutch Oscillation Settings")]
    public float dutchMaxAmplitude = 0.5f; // à combien ça peut monter
    public float dutchFrequency = 5f;      // oscillations par seconde

    private CinemachineVirtualCamera _vcam;
    private CinemachineImpulseSource _impulseSource;
    private LensDistortion _lensDistortion;
    private float _startFOV;
    private float _timePassed;
    private bool _isCharging;
    private float _accumulatedForce;

    public void Setup(CinemachineVirtualCamera vcam, CinemachineImpulseSource impulse)
    {
        _vcam = vcam;
        _impulseSource = impulse;
        _startFOV = vcam.m_Lens.FieldOfView;

        var volumeSettings = _vcam.GetComponent<CinemachineVolumeSettings>();
        if (volumeSettings != null && volumeSettings.m_Profile.TryGet(out _lensDistortion))
        {
            _lensDistortion.intensity.overrideState = true;
            _lensDistortion.intensity.value = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!_isCharging) return;

        _timePassed += Time.deltaTime;
        _accumulatedForce = startShakeForce + _timePassed * shakeMultiplier;
    
        if (_vcam != null)
        {
            float amplitude = Mathf.Min(_timePassed * dutchMaxAmplitude, dutchMaxAmplitude);
            float dutchValue = Mathf.Sin(Time.time * dutchFrequency * Mathf.PI * 2f) * amplitude;
            _vcam.m_Lens.Dutch = dutchValue;
        }
        
        float targetFOV = _startFOV + _timePassed * fovMultiplier;
        _vcam.m_Lens.FieldOfView = Mathf.Min(targetFOV, maxFOV);

        if (_lensDistortion != null)
        {
            float targetDistortion = _timePassed * distortionMax;
            _lensDistortion.intensity.value = Mathf.Max(targetDistortion, distortionLimit);
        }
    }

    public void ReceiveGroundPoundEvent(Enum state, int level)
    {
        if (state.Equals(PlayerStates.GroundPoundState.StartGroundPound))
        {
            _timePassed = 0f;
            _accumulatedForce = 0f;
            _isCharging = true;
        }

        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            _isCharging = false;
            TriggerImpulse(_accumulatedForce);
            ResetFOV();
            ResetDistortion();
        }
    }

    private void TriggerImpulse(float force)
    {
        _impulseSource?.GenerateImpulse(Vector3.one * force);
    }

    private void ResetFOV()
    {
        float currentFOV = _vcam.m_Lens.FieldOfView;
        DOTween.To(() => currentFOV,
                   x => _vcam.m_Lens.FieldOfView = x,
                   _startFOV,
                   fovDuration)
               .SetEase(Ease.OutSine);
        _vcam.m_Lens.Dutch = 0f;
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
} 
