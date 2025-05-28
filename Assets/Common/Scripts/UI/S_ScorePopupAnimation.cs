using UnityEngine;
using TMPro;
using DG.Tweening;

public class S_ScorePopupAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How long the popup stays visible (seconds).")]
    [SerializeField] private float lifetime = 1f;

    [Tooltip("Scale applied at the moment the popup appears.")]
    [SerializeField] private float punchScale = 1.3f;

    [Tooltip("Minimum interval (in seconds) between Show() calls to avoid spam.")]
    [SerializeField] private float minTriggerInterval = 0.05f;

    private TextMeshProUGUI text;
    private Tween fadeTween;
    private Tween scaleTween;

    private Color originalColor;
    private Vector3 originalScale;
    private float lastShowTime = -999f;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
        originalScale = transform.localScale;

        Color c = originalColor;
        c.a = 0f;
        text.color = c; // start invisible
    }

    /// <summary>
    /// Updates the number and restarts the animation.
    /// Ignores call if it's within minTriggerInterval.
    /// </summary>
    public void Show(int value)
    {
        float currentTime = Time.unscaledTime;
        if (currentTime - lastShowTime < minTriggerInterval)
            return;

        lastShowTime = currentTime;

        text.text = $"+{value} pts";

        // Kill existing tweens
        fadeTween?.Kill();
        scaleTween?.Kill();

        // Set start state
        transform.localScale = originalScale * punchScale;
        Color c = originalColor;
        c.a = 1f;
        text.color = c;

        // Animate scale and fade
        scaleTween = transform.DOScale(originalScale, lifetime).SetEase(Ease.OutCubic);
        fadeTween  = DOTween.ToAlpha(() => text.color, x => text.color = x, 0f, lifetime).SetEase(Ease.OutCubic);
    }
}