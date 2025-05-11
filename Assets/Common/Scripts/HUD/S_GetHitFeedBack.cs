using System;
using UnityEngine;
using UnityEngine.UI;

public class S_GetHitFeedback : MonoBehaviour
{
    [Header("References")]
    [Tooltip("UI Image with a material that has the _Progress property")]
    public Image feedbackImage;

    [Header("Normal Mode")]
    [Tooltip("Progress value range for normal mode (min, max)")]
    public Vector2 progressRange = new Vector2(0f, 1f);
    [Tooltip("Number of hits to reach max progress")]
    public int hitsToMax = 5;
    [Tooltip("Seconds to wait before falling back to min progress")]
    public float fallDelay = 2f;
    [Tooltip("Lerp speed per second to approach max in normal mode")]
    public float approachToXSpeed = 2f;
    [Tooltip("Lerp speed per second to return to min in normal mode")]
    public float approachToYSpeed = 1f;

    [Header("One Hit Mode")]
    [Tooltip("Progress value range during one hit mode (min, max)")]
    public Vector2 oneHitProgressRange = new Vector2(0f, 1f);
    [Tooltip("Lerp speed per second to approach max when entering one hit mode")]
    public float oneHitApproachSpeed = 3f;
    [Tooltip("Lerp speed per second to return to min when exiting one hit mode")]
    public float oneHitReturnSpeed = 5f;

    private Material runtimeMaterial;
    private float currentProgress;
    private float targetProgress;
    private int hitCount;
    private float lastHitTime;
    private bool isInOneHitMode;

    private void Start()
    {
        // Clone material to isolate runtime changes from the original asset
        if (feedbackImage != null && feedbackImage.material != null)
        {
            runtimeMaterial = Instantiate(feedbackImage.material);
            feedbackImage.material = runtimeMaterial;
        }

        currentProgress = targetProgress = progressRange.x;
        SetMaterialProgress(currentProgress);

        // Subscribe to player health state events
        S_PlayerStateObserver.Instance.OnPlayerHealthStateEvent += HandlePlayerStateEvent;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (S_PlayerStateObserver.Instance != null)
            S_PlayerStateObserver.Instance.OnPlayerHealthStateEvent -= HandlePlayerStateEvent;
    }

    private void Update()
    {
        // In normal mode, after delay, start falling back to min
        if (!isInOneHitMode &&
            Time.time - lastHitTime > fallDelay &&
            !Mathf.Approximately(targetProgress, progressRange.x))
        {
            targetProgress = progressRange.x;
        }

        if (!Mathf.Approximately(currentProgress, targetProgress))
        {
            // Choose lerp speed based on mode and direction
            float lerpSpeed = isInOneHitMode
                ? (currentProgress < targetProgress ? oneHitApproachSpeed : oneHitReturnSpeed)
                : (currentProgress < targetProgress ? approachToXSpeed : approachToYSpeed);

            // Smoothly interpolate towards the target
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, lerpSpeed * Time.deltaTime);
            SetMaterialProgress(currentProgress);

            // Only update hit count proportionally in normal mode
            if (!isInOneHitMode)
            {
                float tNorm = Mathf.InverseLerp(progressRange.x, progressRange.y, currentProgress);
                hitCount = Mathf.Clamp(Mathf.RoundToInt(tNorm * hitsToMax), 0, hitsToMax);
            }
        }
    }

    private void HandlePlayerStateEvent(Enum state)
    {
        var healthState = (PlayerStates.PlayerHealthState)state;
        switch (healthState)
        {
            case PlayerStates.PlayerHealthState.PlayerGetHit:
                OnPlayerGetHit();
                break;
            case PlayerStates.PlayerHealthState.PlayerOneHitModeStart:
                OnOneHitModeStart();
                break;
            case PlayerStates.PlayerHealthState.PlayerOneHitModeEnd:
                OnOneHitModeEnd();
                break;
        }
    }

    private void OnPlayerGetHit()
    {
        if (isInOneHitMode) return;

        // Increase hit count and update target progress
        hitCount = Mathf.Clamp(hitCount + 1, 0, hitsToMax);
        float t = hitCount / (float)hitsToMax;
        targetProgress = Mathf.Lerp(progressRange.x, progressRange.y, t);
        lastHitTime = Time.time;
    }

    private void OnOneHitModeStart()
    {
        isInOneHitMode = true;
        hitCount = 0;                     // reset normal hits
        targetProgress = oneHitProgressRange.y;
        lastHitTime = Time.time;
    }

    private void OnOneHitModeEnd()
    {
        isInOneHitMode = false;
        hitCount = 0;                     // clear state on exit
        targetProgress = oneHitProgressRange.x;
        lastHitTime = Time.time;
    }

    private void SetMaterialProgress(float value)
    {
        if (runtimeMaterial != null)
            runtimeMaterial.SetFloat("_Progress", value);
    }
}
