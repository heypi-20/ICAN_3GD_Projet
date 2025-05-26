using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public EnemyType enemyType;            // Used for tracking and pooling
    public float health;
    public float enemyDamage;
    public GameObject WeakPoint;           // GameObject to activate/deactivate the weak point
    public GameObject energyPoint;
    public int energyDropQuantity;
    public int energyDropPerHitWeakness;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public EventReference enemyKillEvent;

    [Header("Health Feedback Overlay")]
    public Renderer targetRenderer;        // Main renderer for damage feedback
    public Renderer weakPointRenderer;     // Renderer for weak point feedback
    public Material feedbackMatTemplate;   // Template material supporting transparency
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float feedbackAlpha = 0.8f;
    public float feedbackDuration = 0.2f;

    // Internal structure: each Renderer maps to its feedback data
    private class FeedbackData
    {
        public Material instance;
        public Coroutine coroutine;
    }
    private readonly Dictionary<Renderer, FeedbackData> feedbackMap = new Dictionary<Renderer, FeedbackData>();

    private EventInstance killEventInstance;
    public float currentHealth;
    private bool isDead = false;

    public static event Action<EnemyType> OnEnemyKilled;  // Global kill event
    public event Action<EnemyBase> OnKilled;              // Instance kill event

    protected virtual void OnEnable()
    {
        // Reset health and state
        currentHealth = health;
        isDead = false;

        // Automatically find weak point child if not set
        FindWeakPoint();

        // Activate the weak point object by default
        if (WeakPoint != null)
            WeakPoint.SetActive(true);

        // Register feedback materials for renderers (only once per renderer)
        RegisterFeedbackRenderer(targetRenderer);
        RegisterFeedbackRenderer(weakPointRenderer);
    }

    // OnDisable is virtual so subclasses can override
    protected virtual void OnDisable()
    {
        // Clear instance kill callback
        OnKilled = null;

        // Stop all feedback coroutines and reset alpha
        foreach (var data in feedbackMap.Values)
        {
            if (data.coroutine != null)
                StopCoroutine(data.coroutine);

            Color color = data.instance.color;
            color.a = 0f;
            data.instance.color = color;
            data.coroutine = null;
        }

        // Deactivate weak point object
        if (WeakPoint != null)
            WeakPoint.SetActive(false);
    }

    /// <summary>
    /// Find the weak point child by tag and assign it
    /// </summary>
    private void FindWeakPoint()
    {
        if (WeakPoint != null)
            return;

        foreach (var t in transform.GetComponentsInChildren<Transform>())
        {
            if (t.CompareTag("WeakPoint"))
            {
                WeakPoint = t.gameObject;
                break;
            }
        }
    }

    /// <summary>
    /// Register a feedback material instance to the given renderer (only once)
    /// </summary>
    private void RegisterFeedbackRenderer(Renderer renderer)
    {
        if (renderer == null || feedbackMatTemplate == null || feedbackMap.ContainsKey(renderer))
            return;

        // Check if the material is already added
        foreach (var mat in renderer.materials)
        {
            if (mat.name.Contains(feedbackMatTemplate.name))
            {
                feedbackMap[renderer] = new FeedbackData { instance = mat, coroutine = null };
                return;
            }
        }

        // Create a new instance and append it
        Material instance = new Material(feedbackMatTemplate);
        instance.color = new Color(fullHealthColor.r, fullHealthColor.g, fullHealthColor.b, 0f);

        var mats = renderer.materials;
        var newMats = new Material[mats.Length + 1];
        mats.CopyTo(newMats, 0);
        newMats[mats.Length] = instance;
        renderer.materials = newMats;

        feedbackMap[renderer] = new FeedbackData { instance = instance, coroutine = null };
    }

    /// <summary>
    /// Trigger feedback flash on the appropriate renderer based on hit type
    /// hit == 1 -> weak point renderer; hit == 0 -> main renderer
    /// </summary>
    private void TriggerHealthFeedback(int hit)
    {
        float ratio = Mathf.Clamp01(currentHealth / health);
        Color color = Color.Lerp(lowHealthColor, fullHealthColor, ratio);
        color.a = feedbackAlpha;

        Renderer renderer = (hit == 1 && weakPointRenderer != null) ? weakPointRenderer : targetRenderer;
        if (renderer == null || !feedbackMap.TryGetValue(renderer, out var data))
            return;

        if (data.coroutine != null)
            StopCoroutine(data.coroutine);

        data.coroutine = StartCoroutine(AnimateFeedbackColor(data.instance, color));
    }

    private IEnumerator AnimateFeedbackColor(Material mat, Color visibleColor)
    {
        float half = feedbackDuration / 2f;
        float t = 0f;

        // Fade in
        while (t < half)
        {
            float f = t / half;
            mat.color = Color.Lerp(new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0f), visibleColor, f);
            t += Time.deltaTime;
            yield return null;
        }

        // Fade out
        t = 0f;
        while (t < half)
        {
            float f = t / half;
            mat.color = Color.Lerp(visibleColor, new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0f), f);
            t += Time.deltaTime;
            yield return null;
        }

        // Ensure fully transparent
        mat.color = new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0f);
    }

    /// <summary>
    /// Reduce health, trigger feedback, VFX, and death
    /// </summary>
    public void ReduceHealth(float amount, int dropBonus, Vector3 hitPosition = default, int hit = 0)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        if (hitPosition == default)
            hitPosition = transform.position;

        if (hit == 1 && WeakPoint != null)
        {
            DropItems(energyDropPerHitWeakness, WeakPoint.transform.position);
        }

        TriggerHealthFeedback(hit);

        if (enemyGetHitVFX != null)
            S_VFXPoolManager.Instance.SpawnVFX(enemyGetHitVFX, hitPosition, transform.rotation, 3f);

        if (currentHealth <= 0)
            EnemyDied(dropBonus);
    }

    /// <summary>
    /// Handle enemy death: VFX, events, drops, and pool deactivate
    /// </summary>
    public void EnemyDied(int dropBonus)
    {
        if (isDead)
            return;
        isDead = true;

        if (enemyDeathVFX != null)
            S_VFXPoolManager.Instance.SpawnVFX(enemyDeathVFX, transform.position, transform.rotation, 3f);

        OnEnemyKilled?.Invoke(enemyType);
        OnKilled?.Invoke(this);

        killEventInstance = RuntimeManager.CreateInstance(enemyKillEvent);
        RuntimeManager.AttachInstanceToGameObject(killEventInstance, transform, GetComponent<Rigidbody>());
        killEventInstance.start();

        DropItems(energyDropQuantity + dropBonus);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Drop energy items with random offset
    /// </summary>
    public void DropItems(float dropQuantity, Vector3 selfPosition = default)
    {
        bool useDefault = (selfPosition == default);
        if (useDefault)
            selfPosition = transform.position;

        for (int i = 0; i < dropQuantity; i++)
        {
            Vector3 spawnPos = useDefault
                ? selfPosition + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f))
                : selfPosition;
            Vector3 dir = (spawnPos - selfPosition).normalized;
            S_EnergyPointPoolManager.Instance.QueueEnergyPoint(energyPoint, spawnPos, dir);
        }
    }
}
