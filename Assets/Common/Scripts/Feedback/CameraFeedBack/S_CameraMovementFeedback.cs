// S_CameraMovementFeedback.cs
using System;
using System.Collections;
using UnityEngine;
using Cinemachine;

public class S_CameraMovementFeedback : MonoBehaviour
{
    [Header("Dutch Effect Settings")]
    public float maxDutchAngle = 5f;
    public float dutchDuration = 0.5f;
    public float resetDuration = 0.5f;
    public AnimationCurve dutchCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve resetCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Pitch Tilt Settings")]
    public float maxPitchAngle = 8f;
    public float pitchDuration = 0.3f;
    public bool autoResetTilt = true;
    public AnimationCurve pitchToAngleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve pitchToZeroCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CinemachineVirtualCamera _vcam;
    private CinemachineRecomposer _recomposer;
    private Coroutine _currentDutchCoroutine;
    private Coroutine _currentTiltCoroutine;
    private float _currentDutchAngle;
    private float _currentTiltAngle;
    private Vector2 _previousInputDirection;

    public void Setup(CinemachineVirtualCamera vcam, CinemachineRecomposer recomposer)
    {
        _vcam = vcam;
        _recomposer = recomposer;
    }

    public void ReceiveMoveEvent(Enum state, Vector2 inputDirection)
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

    private void CameraDutch(Vector2 direction)
    {
        float targetDutch = 0f;

        if (direction.x > 0.1f) targetDutch = -maxDutchAngle;
        else if (direction.x < -0.1f) targetDutch = maxDutchAngle;

        if (_currentDutchCoroutine != null)
            StopCoroutine(_currentDutchCoroutine);

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
            _vcam.m_Lens.Dutch = _currentDutchAngle;
            yield return null;
        }

        _currentDutchAngle = targetAngle;
        _vcam.m_Lens.Dutch = _currentDutchAngle;
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

        bool xChanged = (Mathf.Abs(newDirection.x) > threshold) != (Mathf.Abs(_previousInputDirection.x) > threshold);
        bool yChanged = (Mathf.Abs(newDirection.y) > threshold) != (Mathf.Abs(_previousInputDirection.y) > threshold);

        _previousInputDirection = newDirection;

        return xChanged || yChanged;
    }
}