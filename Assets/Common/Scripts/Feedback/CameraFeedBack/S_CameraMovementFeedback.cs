using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class S_CameraMovementFeedback : MonoBehaviour
{
    #region Inspector
    [Header("Dutch Effect Settings")]
    public float maxDutchAngle = 5f;
    public float dutchDuration = 0.5f;
    public float resetDuration = 0.5f;
    public AnimationCurve dutchCurve  = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve resetCurve  = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Pitch Tilt Settings")]
    public float maxPitchAngle = 8f;
    public float pitchDuration = 0.3f;
    public bool  autoResetTilt = true;
    public AnimationCurve pitchToAngleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve pitchToZeroCurve  = AnimationCurve.EaseInOut(0, 0, 1, 1);
    #endregion

    private CinemachineVirtualCamera _vcam;
    private CinemachineRecomposer _recomposer;

    private Coroutine _currentDutchCoroutine;
    private Coroutine _currentTiltCoroutine;

    private float _currentDutchAngle;
    private float _currentTiltAngle;
    private Vector2 _previousInputDirection = Vector2.zero;

    #region Setup
    public void Setup(CinemachineVirtualCamera vcam, CinemachineRecomposer recomposer)
    {
        _vcam       = vcam;
        _recomposer = recomposer;
    }
    #endregion

    #region Public Event
    public void ReceiveMoveEvent(Enum state, Vector2 inputDirection)
    {
        if (HasHorizontalSignChanged(inputDirection))
            CameraDutch(inputDirection);

        // Vertical input controls pitch tilt
        if (inputDirection.y > 0.1f)
            CameraTilt(-maxPitchAngle);
        else if (inputDirection.y < -0.1f)
            CameraTilt( maxPitchAngle);
        else
            CameraTilt(0f);
    }
    #endregion

    #region Dutch Logic
    private void CameraDutch(Vector2 direction)
    {
        float targetDutch = 0f;

        if (direction.x > 0.1f)  targetDutch = -maxDutchAngle;
        else if (direction.x < -0.1f) targetDutch =  maxDutchAngle;

        // Avoid restarting if target ≈ current
        if (Mathf.Abs(targetDutch - _currentDutchAngle) < 0.01f) return;

        if (_currentDutchCoroutine != null)
            StopCoroutine(_currentDutchCoroutine);

        _currentDutchCoroutine = StartCoroutine(ApplyDutchEffect(targetDutch));
    }

    private IEnumerator ApplyDutchEffect(float targetAngle)
    {
        float startAngle = _currentDutchAngle;
        float duration   = (targetAngle == 0) ? resetDuration : dutchDuration;
        float elapsed    = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = (targetAngle == 0) ? resetCurve.Evaluate(t) : dutchCurve.Evaluate(t);

            _currentDutchAngle  = Mathf.Lerp(startAngle, targetAngle, t);
            if (_vcam) _vcam.m_Lens.Dutch = _currentDutchAngle;

            yield return null;
        }

        _currentDutchAngle  = targetAngle;
        if (_vcam) _vcam.m_Lens.Dutch = _currentDutchAngle;
    }
    #endregion

    #region Tilt Logic
    private void CameraTilt(float targetAngle)
    {
        if (Mathf.Abs(targetAngle - _currentTiltAngle) < 0.01f) return;

        if (_currentTiltCoroutine != null)
            StopCoroutine(_currentTiltCoroutine);

        _currentTiltCoroutine = StartCoroutine(ApplyTiltEffectWithReturn(targetAngle));
    }

    private IEnumerator ApplyTiltEffectWithReturn(float targetAngle)
    {
        float startAngle = _currentTiltAngle;
        float elapsed    = 0f;

        // Move toward target pitch
        while (elapsed < pitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pitchDuration);
            float curveT = pitchToAngleCurve.Evaluate(t);

            _currentTiltAngle = Mathf.Lerp(startAngle, targetAngle, curveT);
            if (_recomposer) _recomposer.m_Tilt = _currentTiltAngle;

            yield return null;
        }

        _currentTiltAngle = targetAngle;

        if (!autoResetTilt) yield break;

        // Small delay before auto-reset
        yield return new WaitForSeconds(0.05f);

        // Return to neutral
        startAngle = _currentTiltAngle;
        elapsed    = 0f;

        while (elapsed < pitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pitchDuration);
            float curveT = pitchToZeroCurve.Evaluate(t);

            _currentTiltAngle = Mathf.Lerp(startAngle, 0f, curveT);
            if (_recomposer) _recomposer.m_Tilt = _currentTiltAngle;

            yield return null;
        }

        _currentTiltAngle = 0f;
        if (_recomposer) _recomposer.m_Tilt = 0f;
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Returns true when the sign of horizontal input flips beyond threshold.
    /// </summary>
    private bool HasHorizontalSignChanged(Vector2 newDir)
    {
        const float threshold = 0.1f;

        int prevSign = Mathf.Abs(_previousInputDirection.x) > threshold ? Math.Sign(_previousInputDirection.x) : 0;
        int newSign  = Mathf.Abs(newDir.x)               > threshold ? Math.Sign(newDir.x)               : 0;

        _previousInputDirection = newDir;
        return prevSign != newSign;
    }
    #endregion
}
