using System;
using UnityEngine;
using UnityEngine.UI;

public class S_GetHitFeedBack : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign the UI Image whose material has the _Progress property")]
    public Image feedbackImage;

    [Header("Normal Mode")]
    [Tooltip("Progress Range (min, max)")]
    public Vector2 progressRange = new Vector2(0f, 1f);
    [Tooltip("连续命中次数达到此值时，Progress 达到最大值")]
    public int hitsToMax = 5;
    [Tooltip("持续多少秒后开始归拢到最小值")]
    public float fallDelay = 2f;
    [Tooltip("Lerp 百分比／秒，用于向最大值靠近 (Normal Mode)")]
    public float approachToXSpeed = 2f;
    [Tooltip("Lerp 百分比／秒，用于向最小值归拢 (Normal Mode)")]
    public float approachToYSpeed = 1f;

    [Header("One Hit Mode")]
    [Tooltip("Progress Range (min, max) During One Hit Mode")]
    public Vector2 oneHitProgressRange = new Vector2(0f, 1f);
    [Tooltip("Lerp 百分比／秒，当进入 One Hit Mode 时向最大值靠近")]
    public float oneHitApproachSpeed = 3f;
    [Tooltip("Lerp 百分比／秒，当退出 One Hit Mode 时向最小值归拢")]
    public float oneHitReturnSpeed = 5f;

    private Material runtimeMaterial;
    private float currentProgress;
    private float targetProgress;
    private int hitCount;
    private float lastHitTime;
    private bool isInOneHitMode;

    private void Start()
    {
        // 克隆材质实例，避免修改原 Asset
        if (feedbackImage != null && feedbackImage.material != null)
        {
            runtimeMaterial = Instantiate(feedbackImage.material);
            feedbackImage.material = runtimeMaterial;
        }

        currentProgress = targetProgress = progressRange.x;
        SetMaterialProgress(currentProgress);

        S_PlayerStateObserver.Instance.OnPlayerHealthStateEvent += handlePlayerStateEvent;
    }

    private void OnDestroy()
    {
        if (S_PlayerStateObserver.Instance != null)
            S_PlayerStateObserver.Instance.OnPlayerHealthStateEvent -= handlePlayerStateEvent;
    }

    private void Update()
    {
        // Normal Mode 下超过延迟则归拢到 Normal min
        if (!isInOneHitMode &&
            Time.time - lastHitTime > fallDelay &&
            !Mathf.Approximately(targetProgress, progressRange.x))
        {
            targetProgress = progressRange.x;
        }

        if (!Mathf.Approximately(currentProgress, targetProgress))
        {
            // 根据当前模式和增减方向选择对应的 Lerp 速率
            float lerpPct;
            if (isInOneHitMode)
                lerpPct = (currentProgress < targetProgress)
                    ? oneHitApproachSpeed
                    : oneHitReturnSpeed;
            else
                lerpPct = (currentProgress < targetProgress)
                    ? approachToXSpeed
                    : approachToYSpeed;

            currentProgress = Mathf.Lerp(currentProgress, targetProgress, lerpPct * Time.deltaTime);
            SetMaterialProgress(currentProgress);

            // 只有 Normal Mode 下才动态对 hitCount 进行映射
            if (!isInOneHitMode)
            {
                float tNorm = Mathf.InverseLerp(progressRange.x, progressRange.y, currentProgress);
                hitCount = Mathf.Clamp(Mathf.RoundToInt(tNorm * hitsToMax), 0, hitsToMax);
            }
        }
    }

    private void handlePlayerStateEvent(Enum state)
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
        if (isInOneHitMode)
            return;

        // 正常累积
        hitCount = Mathf.Clamp(hitCount + 1, 0, hitsToMax);
        float t = hitCount / (float)hitsToMax;
        targetProgress = Mathf.Lerp(progressRange.x, progressRange.y, t);
        lastHitTime = Time.time;
    }

    private void OnOneHitModeStart()
    {
        isInOneHitMode = true;
        hitCount = 0;  // 忽略之前的累积
        targetProgress = oneHitProgressRange.y;
        lastHitTime = Time.time;
    }

    private void OnOneHitModeEnd()
    {
        isInOneHitMode = false;
        hitCount = 0;
        targetProgress = oneHitProgressRange.x;
        lastHitTime = Time.time;
    }

    private void SetMaterialProgress(float value)
    {
        if (runtimeMaterial != null)
            runtimeMaterial.SetFloat("_Progress", value);
    }
}
