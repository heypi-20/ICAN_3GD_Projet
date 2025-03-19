using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class S_CameraFeedBack : MonoBehaviour
{
    public CinemachineVirtualCamera _cinemachineVirtualCamera; 
    private CinemachineInputProvider _cinemachineInputProvider;
    
    [Header("Dutch Effect Settings")]
    public float maxDutchAngle = 5f; // Maximum Dutch angle
    public float dutchDuration = 0.5f; // Time to reach Dutch target
    public float resetDuration = 0.5f; // Time to return to zero

    public AnimationCurve dutchCurve = AnimationCurve.Linear(0, 0, 1, 1); // Custom curve for entering Dutch
    public AnimationCurve resetCurve = AnimationCurve.Linear(0, 0, 1, 1); // Custom curve for resetting Dutch
    private Coroutine _currentDutchCoroutine;
    private float _currentDutchAngle = 0f; // Stores the current Dutch angle

    
    private Vector2 _previousInputDirection = Vector2.zero; // Store previous movement direction

    private void OnEnable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent += ReceiveMoveEvent;
        }
        else
        {
            StartCoroutine(WaitForObserver());
        }
    }



    private IEnumerator WaitForObserver()
    {
        float timeout = 3f; // Maximum wait time (3 seconds)
        float elapsedTime = 0f;

        while (S_PlayerStateObserver.Instance == null)
        {
            if (elapsedTime >= timeout)
            {
                Debug.LogError("S_PlayerStateObserver.Instance not found after waiting " + timeout + " seconds.");
                yield break; // Exit coroutine to prevent an infinite loop
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


    private void EnableCamera(bool state)
    {
        if (_cinemachineInputProvider != null)
            _cinemachineInputProvider.enabled = state;
    }

    /// <summary>
    /// Handles movement state changes received from the observer.
    /// </summary>
    private void ReceiveMoveEvent(Enum movingState, Vector2 inputDirection)
    {
        // Check if movement direction has changed
        if (HasDirectionChanged(inputDirection))
        {
            CameraDutch(inputDirection);
        }
    }

    /// <summary>
    /// Checks if the movement direction has changed significantly.
    /// </summary>
    private bool HasDirectionChanged(Vector2 newDirection)
    {
        float threshold = 0.1f;

        // Determine if the X or Y direction has crossed the ±0.1 boundary
        bool xDirectionChanged = (Mathf.Abs(newDirection.x) > threshold) != (Mathf.Abs(_previousInputDirection.x) > threshold);
        bool yDirectionChanged = (Mathf.Abs(newDirection.y) > threshold) != (Mathf.Abs(_previousInputDirection.y) > threshold);

        // Update previous direction
        _previousInputDirection = newDirection;

        return xDirectionChanged || yDirectionChanged;
    }

    
    #region CameraDutch
    private void CameraDutch(Vector2 direction)
    {
        float targetDutch = 0f;

        // Determine target Dutch angle based on movement direction
        if (direction.x > 0.1f) targetDutch = -maxDutchAngle;  // Moving right
        else if (direction.x < -0.1f) targetDutch = maxDutchAngle;  // Moving left
        else if (direction.x == 0) targetDutch = 0f;  // Reset when no horizontal movement

        // Start transition coroutine
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
            t = (targetAngle == 0) ? resetCurve.Evaluate(t) : dutchCurve.Evaluate(t); // Use different curves

            _currentDutchAngle = Mathf.Lerp(startAngle, targetAngle, t);

            if (_cinemachineVirtualCamera != null)
            {
                _cinemachineVirtualCamera.m_Lens.Dutch = _currentDutchAngle;
            }

            yield return null;
        }

        _currentDutchAngle = targetAngle;
        if (_cinemachineVirtualCamera != null)
        {
            _cinemachineVirtualCamera.m_Lens.Dutch = _currentDutchAngle;
        }
    }
    
    #endregion

    private void OnDisable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnMoveStateEvent -= ReceiveMoveEvent;
        }
    }
}
