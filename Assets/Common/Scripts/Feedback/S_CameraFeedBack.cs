using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

public class S_CameraFeedBack : MonoBehaviour
{
    public CinemachineVirtualCamera _cinemachineVirtualCamera; 
    private CinemachineInputProvider _cinemachineInputProvider;
    //private CinemachineComposer _composer;
    
    [Header("Dutch Effect Settings")]
    public float maxDutchAngle = 5f; 
    public float dutchDuration = 0.5f;
    public float resetDuration = 0.5f;
    public AnimationCurve dutchCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve resetCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private Coroutine _currentDutchCoroutine;
    private float _currentDutchAngle = 0f;
    private Vector2 _previousInputDirection = Vector2.zero;
    
    
    [Header("Pitch Tilt Settings")]
    public float maxPitchAngle = 8f;
    public float pitchDuration = 0.3f;
    public bool autoResetTilt = true;
    public AnimationCurve pitchToAngleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve pitchToZeroCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("CAC Shake")]
    public float CameraShakeCAC_Lvl1;
    public float CameraShakeCAC_Lvl2;
    public float CameraShakeCAC_Lvl3;
    public float CameraShakeCAC_Lvl4;
    private CinemachineImpulseSource _impulseSource;
    
    [Header("Pillonage Shake")]
    public float CameraShakePillonage;
    public float stopDuration = 0.2f;
    public int normalFPS = 60;     // FPS normal du jeu
    public int effectFPS = 24;     // FPS pendant l'effet (plus bas = plus "cinématique")
    public float duration = 0.5f;  // Durée de l'effet en secondes
    private float _timer_of_groundpound;
    private bool isActive = false;
    public float tiltAnglePillonage;
    public float shakeIntensityPillonage;
    public float fallDurationPillonage;
    public float recoverDurationPillonage;

    [Header("Pillonage FOV")] 
    public float fov_multiplicator;

    public float fov_transition_time;

    private float Curent_FOV;
    private float Start_FOV;
    private bool isIncreasing;
    private float timePassed;
        
   
    private CinemachineRecomposer _recomposer;
    private float _currentTiltAngle = 0f;
    private Coroutine _currentTiltCoroutine;
    private Tween currentTween;

    private void Start()
    {
        
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent += ReceiveMoveEvent;
            S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += ReceiveMeleeAttackEvent;
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiveGroudPoundEvevent;
        }
        if (_cinemachineVirtualCamera == null)
        {
            _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        }
        _recomposer = _cinemachineVirtualCamera.GetComponent<CinemachineRecomposer>();
        _impulseSource = _cinemachineVirtualCamera.GetComponent<CinemachineImpulseSource>();
        _cinemachineInputProvider = _cinemachineVirtualCamera.GetComponent<CinemachineInputProvider>();

    }

    private void Update()
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
        {
            EnableCamera(false);
        }
        else
        {
            EnableCamera(true);
        }
    }
    
    private void FixedUpdate()
    {
        _timer_of_groundpound += Time.deltaTime;
        
        if (isIncreasing)
        {
            timePassed += Time.deltaTime;
            _cinemachineVirtualCamera.m_Lens.FieldOfView = Start_FOV + timePassed * fov_multiplicator;
        }
    }

    private void EnableCamera(bool state)
    {
        if (_cinemachineInputProvider != null)
            _cinemachineInputProvider.enabled = state;
    }

    private void ReceiveMoveEvent(Enum movingState, Vector2 inputDirection)
    {
        if (HasDirectionChanged(inputDirection))
        {
            CameraDutch(inputDirection);
        }
        if (inputDirection.y > 0.1f)
            CameraTilt(-maxPitchAngle);   
        else if (inputDirection.y < -0.1f)
            CameraTilt(maxPitchAngle);    
        else
            CameraTilt(0f);   
    }
    
    private void CameraTilt(float targetAngle)
    {
        if (_currentTiltCoroutine != null)
            StopCoroutine(_currentTiltCoroutine);

        _currentTiltCoroutine = StartCoroutine(ApplyTiltEffectWithReturn(targetAngle));
    }

    private IEnumerator ApplyTiltEffectWithReturn(float targetAngle)
    {
        float startAngle = _currentTiltAngle;
        float elapsedTime = 0f;

        // 1. 进入倾斜
        while (elapsedTime < pitchDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / pitchDuration);
            float curveT = pitchToAngleCurve.Evaluate(t);

            _currentTiltAngle = Mathf.Lerp(startAngle, targetAngle, curveT);
            if (_recomposer != null)
                _recomposer.m_Tilt = _currentTiltAngle;

            yield return null;
        }

        _currentTiltAngle = targetAngle;
        
        if (!autoResetTilt)
            yield break;
        
        yield return new WaitForSeconds(0.05f);
        
        startAngle = _currentTiltAngle;
        elapsedTime = 0f;

        while (elapsedTime < pitchDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / pitchDuration);
            float curveT = pitchToZeroCurve.Evaluate(t);

            _currentTiltAngle = Mathf.Lerp(startAngle, 0f, curveT);
            if (_recomposer != null)
                _recomposer.m_Tilt = _currentTiltAngle;

            yield return null;
        }

        _currentTiltAngle = 0f;
        if (_recomposer != null)
            _recomposer.m_Tilt = 0f;
    }
    
    private void ReceiveMeleeAttackEvent(Enum state, int level)
    {
        if (state.Equals(PlayerStates.MeleeState.MeleeAttackHit))
        {
            switch (level)
            {
                case 1:
                    CameraShake(CameraShakeCAC_Lvl1);
                    break;
                case 2:
                    CameraShake(CameraShakeCAC_Lvl2);
                    break;
                case 3:
                    CameraShake(CameraShakeCAC_Lvl3);
                    break;
                case 4:
                    CameraShake(CameraShakeCAC_Lvl4);
                    break;
            }
        }
    }

    private void ReceiveGroudPoundEvevent(Enum state)
    {
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            //CameraFallImpactEffect(tiltAnglePillonage,shakeIntensityPillonage,fallDurationPillonage,recoverDurationPillonage);
            CameraWobbleEffect();
            CameraDutchWobble();
            StartCoroutine(TimeStopCoroutine());
            if (!isActive)
            {
                StartCoroutine(ChangeFPS());
            }

            isIncreasing = false;
            float currentFOV = _cinemachineVirtualCamera.m_Lens.FieldOfView;
            currentTween = DOTween.To(
                () => currentFOV,
                x => _cinemachineVirtualCamera.m_Lens.FieldOfView = x,
                Start_FOV,
                fov_transition_time
            ).SetEase(Ease.OutSine);
        }

        if (state.Equals(PlayerStates.GroundPoundState.StartGroundPound))
        {
            Start_FOV = _cinemachineVirtualCamera.m_Lens.FieldOfView;
            isIncreasing = true;
            timePassed = 0f;
            _timer_of_groundpound = 0f;
        }
    }
    
    private bool HasDirectionChanged(Vector2 newDirection)
    {
        float threshold = 0.1f;

        bool xDirectionChanged = (Mathf.Abs(newDirection.x) > threshold) != (Mathf.Abs(_previousInputDirection.x) > threshold);
        bool yDirectionChanged = (Mathf.Abs(newDirection.y) > threshold) != (Mathf.Abs(_previousInputDirection.y) > threshold);

        _previousInputDirection = newDirection;

        return xDirectionChanged || yDirectionChanged;
    }

    #region CameraDutch
    private void CameraDutch(Vector2 direction)
    {
        float targetDutch = 0f;

        if (direction.x > 0.1f) targetDutch = -maxDutchAngle;
        else if (direction.x < -0.1f) targetDutch = maxDutchAngle;
        else if (direction.x == 0) targetDutch = 0f;

        if (_currentDutchCoroutine != null)
        {
            StopCoroutine(_currentDutchCoroutine);
        }

        _currentDutchCoroutine = StartCoroutine(ApplyDutchEffect(targetDutch));
    }

    private IEnumerator ApplyDutchEffect(float targetAngle)
    {
        float startAngle = _currentDutchAngle;
        float duration = (targetAngle == 0) ? resetDuration : dutchDuration;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = (targetAngle == 0) ? resetCurve.Evaluate(t) : dutchCurve.Evaluate(t);

            _currentDutchAngle = Mathf.Lerp(startAngle, targetAngle, t);
            _cinemachineVirtualCamera.m_Lens.Dutch = _currentDutchAngle;

            yield return null;
        }

        _currentDutchAngle = targetAngle;
        _cinemachineVirtualCamera.m_Lens.Dutch = _currentDutchAngle;
    }
    #endregion
    private void OnDisable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent -= ReceiveMoveEvent;
        }
    }
    public void CameraShake(float intensity = 1f)
    {
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(Vector3.one * intensity);
            //Debug.Log("Ca tape fooooooooooort");
        }
        else
        {
            //Debug.LogWarning("CinemachineImpulseSource is missing on this GameObject.");
        }
    }
    private IEnumerator TimeStopCoroutine()
    {
        // Arrêter le temps
        Time.timeScale = 0.2f;
        
        // Attendre en temps réel (pas en temps de jeu)
        yield return new WaitForSecondsRealtime(stopDuration);
        
        // Reprendre le temps normal
        Time.timeScale = 1f;
    }
    private IEnumerator ChangeFPS()
        {
            isActive = true;
            
            
            // Applique le FPS réduit
            Application.targetFrameRate = effectFPS;
            //Debug.Log("Baisse Fps");
            
            // Attend la durée spécifiée
            yield return new WaitForSecondsRealtime(duration);
            
            // Rétablit le FPS normal
            Application.targetFrameRate = -1;
            //Debug.Log("Débloooouér sa mèèèèèèère");
            
            isActive = false;
        }

    public void CameraWobbleEffect(int wobbleCount = 4, float wobbleAngle = 6f, float wobbleSpeed = 0.05f, float recoverDuration = 0.1f)
    {
        if (_currentTiltCoroutine != null)
            StopCoroutine(_currentTiltCoroutine);

        _currentTiltCoroutine = StartCoroutine(WobbleCoroutine(wobbleCount, wobbleAngle, wobbleSpeed, recoverDuration));
    }

    private IEnumerator WobbleCoroutine(int count, float angle, float speed, float recoverDuration)
    {
        float elapsed = 0f;

        for (int i = 0; i < count; i++)
        {
            float target = (i % 2 == 0) ? angle : -angle;
            float start = _recomposer != null ? _recomposer.m_Tilt : 0f;
            elapsed = 0f;

            while (elapsed < speed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / speed;
                float tilt = Mathf.Lerp(start, target, pitchToAngleCurve.Evaluate(t));

                if (_recomposer != null)
                    _recomposer.m_Tilt = tilt;

                yield return null;
            }
        }

        // Retour progressif à 0
        float endAngle = _recomposer.m_Tilt;
        elapsed = 0f;

        while (elapsed < recoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoverDuration;
            float tilt = Mathf.Lerp(endAngle, 0f, pitchToZeroCurve.Evaluate(t));

            if (_recomposer != null)
                _recomposer.m_Tilt = tilt;

            yield return null;
        }

        _currentTiltAngle = 0f;
    }
    public void CameraDutchWobble(int wobbleCount = 4, float wobbleAngle = 6f, float wobbleSpeed = 0.05f, float recoverDuration = 0.1f)
    {
        if (_currentDutchCoroutine != null)
            StopCoroutine(_currentDutchCoroutine);

        _currentDutchCoroutine = StartCoroutine(DutchWobbleCoroutine(wobbleCount, wobbleAngle, wobbleSpeed, recoverDuration));
    }

    private IEnumerator DutchWobbleCoroutine(int count, float angle, float speed, float recoverDuration)
    {
        float elapsed = 0f;

        for (int i = 0; i < count; i++)
        {
            float target = (i % 2 == 0) ? angle : -angle;
            float start = _cinemachineVirtualCamera.m_Lens.Dutch;

            // Démarre directement l’effet visuel pour la première frame
            _cinemachineVirtualCamera.m_Lens.Dutch = target;
            yield return null;

            elapsed = 0f;

            while (elapsed < speed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / speed;
                float dutch = Mathf.Lerp(target, -target, dutchCurve.Evaluate(t)); // oscille à partir de target

                _cinemachineVirtualCamera.m_Lens.Dutch = dutch;

                yield return null;
            }
        }

        // Retour à 0 smooth
        float endDutch = _cinemachineVirtualCamera.m_Lens.Dutch;
        elapsed = 0f;

        while (elapsed < recoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoverDuration;
            float dutch = Mathf.Lerp(endDutch, 0f, resetCurve.Evaluate(t));

            _cinemachineVirtualCamera.m_Lens.Dutch = dutch;

            yield return null;
        }

        _currentDutchAngle = 0f;
    }
}