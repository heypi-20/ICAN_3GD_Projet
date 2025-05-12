using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

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
    public CinemachineImpulseSource _impulseSource;
    //public SignalSourceAsset shortImpulse;
    
    [Header("Pillonage Shake")]
    private float _timer_of_groundpound;
    public float startimpulseshakeforce = 3f;
    private float impulseforceonGround;
    public float _imulseMultiplicator = 0f;
    public float Weak_shake_Multiplayer = 1.5f;
    
    //public SignalSourceAsset PillonageImpulse;
    
    [Header("ChangeFPS")]
    public float stopDuration = 0.2f;
    public int effectFPS = 24;     // FPS pendant l'effet (plus bas = plus "cinématique")
    public float duration = 0.5f; 
    private bool isActive = false;

    [Header("Pillonage FOV")] 
    public float fov_multiplicator;
    public float fov_transition_time;

    private float Curent_FOV;
    private float Start_FOV;
    private bool isIncreasing;
    private float timePassed;

    [Header("CAC_FOV")] 
    public float CAC_fov_Multiplier;
    public float CAC_Fov_transition_time;
    private bool CAC_IsIncreasing;
        
   
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
            impulseforceonGround = startimpulseshakeforce + timePassed * _imulseMultiplicator;
            _cinemachineVirtualCamera.m_Lens.FieldOfView = Start_FOV + timePassed * fov_multiplicator;
        }

        if (CAC_IsIncreasing)
        {
            timePassed += Time.deltaTime;
            _cinemachineVirtualCamera.m_Lens.FieldOfView = Start_FOV + timePassed * CAC_fov_Multiplier;
        }

        if (Curent_FOV >= 130f)
        {
            isIncreasing = false;
        }
    }

    #region CameraMooveFeedback

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
        private bool HasDirectionChanged(Vector2 newDirection)
        {
            float threshold = 0.1f;

            bool xDirectionChanged = (Mathf.Abs(newDirection.x) > threshold) != (Mathf.Abs(_previousInputDirection.x) > threshold);
            bool yDirectionChanged = (Mathf.Abs(newDirection.y) > threshold) != (Mathf.Abs(_previousInputDirection.y) > threshold);

            _previousInputDirection = newDirection;

            return xDirectionChanged || yDirectionChanged;
        }
    
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
        private void OnDisable()
        {
            if (S_PlayerStateObserver.Instance != null)
            {
                S_PlayerStateObserver.Instance.OnMoveStateEvent -= ReceiveMoveEvent;
            }
        }

    #endregion

    #region MeleeAttack

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
            CAC_IsIncreasing = false;
            float currentFOV = _cinemachineVirtualCamera.m_Lens.FieldOfView;
            currentTween = DOTween.To(
                () => currentFOV,
                x => _cinemachineVirtualCamera.m_Lens.FieldOfView = x,
                Start_FOV,
                CAC_Fov_transition_time
            ).SetEase(Ease.OutSine);
        }

        if (state.Equals(PlayerStates.MeleeState.MeleeAttackHitWeakness))
        {
            switch (level)
            {
                case 1:
                    CameraShake(CameraShakeCAC_Lvl1* Weak_shake_Multiplayer);
                    break;
                case 2:
                    CameraShake(CameraShakeCAC_Lvl2 * Weak_shake_Multiplayer);
                    break;
                case 3:
                    CameraShake(CameraShakeCAC_Lvl3 * Weak_shake_Multiplayer);
                    break;
                case 4:
                    CameraShake(CameraShakeCAC_Lvl4* Weak_shake_Multiplayer);
                    break;
            }
            CAC_IsIncreasing = false;
            float currentFOV = _cinemachineVirtualCamera.m_Lens.FieldOfView;
            currentTween = DOTween.To(
                () => currentFOV,
                x => _cinemachineVirtualCamera.m_Lens.FieldOfView = x,
                Start_FOV,
                CAC_Fov_transition_time
            ).SetEase(Ease.OutSine);
        }

        if (state.Equals(PlayerStates.MeleeState.DashingBeforeMelee))
        {
            Start_FOV = _cinemachineVirtualCamera.m_Lens.FieldOfView;
            CAC_IsIncreasing = true;
            timePassed = 0f;
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

    #endregion

    #region GroundPound

    private void ReceiveGroudPoundEvevent(Enum state,int level)
    {
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            ImpulseGroundPound(impulseforceonGround);
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
    public void ImpulseGroundPound(float impulseStrength)
    {
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(impulseStrength);
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

    #endregion
    
}

    