using UnityEngine;
using DG.Tweening; // DOTween

public class S_FeedSeed : EnemyBase
{
    public float respawnCoolDown = 5f;
    private float previousHealth;

    [Header("Scale Animation")]
    [Tooltip("Object to punch-scale when health decreases")]
    public Transform scaleTarget;
    [Tooltip("Punch strength relative to original scale (e.g. 0.2 = ±20%)")]
    public float punchStrength = 0.2f;
    [Tooltip("Total duration of the punch animation")]
    public float punchDuration = 0.5f;
    [Tooltip("How many times it vibrates during the punch")]
    public int vibrato = 10;
    [Tooltip("Elasticity of the punch (0–1)")]
    public float elasticity = 1f;

    // cache original scale so we can restore it
    private Vector3 originalScale;

    private void Start()
    {
        previousHealth = currentHealth;

        if (scaleTarget != null)
            originalScale = scaleTarget.localScale;
    }

    private void OnDisable()
    {
        Invoke(nameof(ReEnableSelf), respawnCoolDown);
    }

    private void ReEnableSelf()
    {
        gameObject.SetActive(true);
        previousHealth = currentHealth;
    }

    private void Update()
    {
        if (currentHealth < previousHealth)
        {
            DropItems(energyDropQuantity);
            TriggerPunchScale();
            previousHealth = currentHealth;
        }
    }

    private void TriggerPunchScale()
    {
        if (scaleTarget == null) return;

        scaleTarget.DOKill(true);
        scaleTarget.localScale = originalScale;

        Vector3 punch = Vector3.one * punchStrength;
        scaleTarget
            .DOPunchScale(punch, punchDuration, vibrato, elasticity)
            .SetUpdate(true)
            .OnComplete(() => scaleTarget.localScale = originalScale);
    }
}