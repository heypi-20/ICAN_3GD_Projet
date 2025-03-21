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
    
    [Header("CAC Shake")]
    private CinemachineImpulseSource _impulseSource;
    public float CameraShakeCAC_Lvl1;
    public float CameraShakeCAC_Lvl2;
    public float CameraShakeCAC_Lvl3;
    public float CameraShakeCAC_Lvl4;
    
    [Header("Pillonage Shake")]
    private float _timer_of_groundpound;
    public float CameraShakePillonage;
    
    public float stopDuration = 0.2f;

    private void OnEnable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent += ReceiveMoveEvent;
        }
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += ReceiveMeleeAttackEvent;
        }
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiGroudPoundEvevent;
        }
        else
        {
            StartCoroutine(WaitForObserver());
        }
    }

    private IEnumerator WaitForObserver()
    {
        float timeout = 3f;
        float elapsedTime = 0f;

        while (S_PlayerStateObserver.Instance == null)
        {
            if (elapsedTime >= timeout)
            {
                Debug.LogError("S_PlayerStateObserver.Instance not found after waiting " + timeout + " seconds.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        S_PlayerStateObserver.Instance.OnMoveStateEvent += ReceiveMoveEvent;
    }

    private void Start()
    {
        if (_cinemachineVirtualCamera == null)
        {
            _cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        _impulseSource = _cinemachineVirtualCamera.GetComponent<CinemachineImpulseSource>();
    }

    private void FixedUpdate()
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
        {
            EnableCamera(false);
        }
        else
        {
            EnableCamera(true);
        }

        _timer_of_groundpound = _timer_of_groundpound + Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            Time.timeScale = 0;
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

    private void ReceiGroudPoundEvevent(Enum state)
    {
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            CameraShake((CameraShakePillonage*_timer_of_groundpound));
        }

        if (state.Equals(PlayerStates.GroundPoundState.StartGroundPound))
        {
            _timer_of_groundpound = 0f;
            StartCoroutine(TimeStopCoroutine());
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
            Debug.Log("Ca tape fooooooooooort");
        }
        else
        {
            Debug.LogWarning("CinemachineImpulseSource is missing on this GameObject.");
        }
    }
    
    
    private IEnumerator TimeStopCoroutine()
    {
        // Arrêter le temps
        Time.timeScale = 0f;
        
        // Attendre en temps réel (pas en temps de jeu)
        yield return new WaitForSecondsRealtime(stopDuration);
        
        // Reprendre le temps normal
        Time.timeScale = 1f;
    }
}