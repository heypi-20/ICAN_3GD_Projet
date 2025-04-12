using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public string enemyName;
    public EnemyType enemyType; // Used for tracking and pooling
    public float enemyDamage;
    public float health;
    public float WeaknessExposureHealth;
    public GameObject WeakPoint;
    public GameObject energyPoint;
    public float energyDropQuantity;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public string enemyGetHitSound;
    public string enemyDeathSound;
    [Header("WeakPoint Visuals")]
    public Material extraWeakPointMaterial;
    public Renderer weakPointRenderer;
    

    private float currentHealth;
    private bool isDead = false;
    private S_ScoreDisplay _s_ScoreDisplay;
    
    //Use for WeakPoint VFX todo: Will be replace by shader
    private Coroutine pulseCoroutine;
    private Color extraMatBaseColor;
    private Material runtimeExtraMat; // Instantiated material used only by this enemy


    public static event Action OnEnemyKillForCombo; // Global event for combo tracking
    public event Action<EnemyBase> OnKilled; // Instance event for pooling

    // Called when the object is enabled (e.g., when reused from the pool)

    private void OnEnable()
    {
        FindWeakPoint();             // Find and disable the weak point
        currentHealth = health;      // Reset health
        isDead = false;              // Reset death flag for pooling reuse
    }

    // Finds the weak point among child objects and disables it.
    private void FindWeakPoint()
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.CompareTag("WeakPoint"))
            {
                WeakPoint = child.gameObject;
                WeakPoint.SetActive(false);
                break;
            }
        }
    }

    // Reduces health by a given amount, shows hit effects, and triggers death if needed.
    public void ReduceHealth(float amount, int DropBonus)
    {
        if (isDead) return;
        currentHealth -= amount;

        // Show hit effect if available.
        if (enemyGetHitVFX != null)
        {
            S_VFXPoolManager.Instance.SpawnVFX(enemyGetHitVFX, transform.position, transform.rotation, 3f);

        }

        // Activate weak point if health is low.
        if (currentHealth <= WeaknessExposureHealth && WeakPoint != null)
        {
            activeWeakPoint();
        }

        // Trigger death if health is zero or below.
        if (currentHealth <= 0)
        {
            EnemyDied(DropBonus);
        }
    }

    private void activeWeakPoint()
    {
        WeakPoint.SetActive(true);

        if (weakPointRenderer != null && extraWeakPointMaterial != null)
        {
            Material[] originalMats = weakPointRenderer.materials;

            // Check if already added (avoid duplicates)
            foreach (var mat in originalMats)
            {
                if (mat.name.StartsWith(extraWeakPointMaterial.name + "_Instance_"))
                    return;
            }

            // Create and assign a unique instance material
            runtimeExtraMat = new Material(extraWeakPointMaterial);
            runtimeExtraMat.name = extraWeakPointMaterial.name + "_Instance_" + GetInstanceID();

            Material[] newMats = new Material[originalMats.Length + 1];
            for (int i = 0; i < originalMats.Length; i++)
            {
                newMats[i] = originalMats[i];
            }
            newMats[newMats.Length - 1] = runtimeExtraMat;

            weakPointRenderer.materials = newMats;

            // Store base color for alpha pulsing
            if (runtimeExtraMat.HasProperty("_BaseColor"))
            {
                extraMatBaseColor = runtimeExtraMat.GetColor("_BaseColor");
                pulseCoroutine = StartCoroutine(PulseMaterialAlpha(runtimeExtraMat));
            }
        }
    }
    
    private IEnumerator PulseMaterialAlpha(Material targetMat)
    {
        float minAlpha = 0.25f;
        float maxAlpha = 1f;
        float speed = 5f;
        float t = 0f;
        bool increasing = true;

        while (!isDead)
        {
            t += Time.deltaTime * speed * (increasing ? 1f : -1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            Color c = extraMatBaseColor;
            c.a = alpha;

            if (targetMat.HasProperty("_BaseColor"))
            {
                targetMat.SetColor("_BaseColor", c);
            }

            if (t >= 1f)
            {
                t = 1f;
                increasing = false;
            }
            else if (t <= 0f)
            {
                t = 0f;
                increasing = true;
            }

            yield return null;
        }
    }
    

    // Handles enemy death: plays effects, triggers events, drops items, and deactivates the object.
    public void EnemyDied(int DropBonus)
    {
        if (isDead) return;
        isDead = true;

        // Play death effect if available.
        if (enemyDeathVFX != null)
        {
            S_VFXPoolManager.Instance.SpawnVFX(enemyDeathVFX, transform.position, transform.rotation, 3f);

        }

        // Trigger global and instance death events.
        OnEnemyKillForCombo?.Invoke();
        OnKilled?.Invoke(this);

        // Play kill sound and drop energy items.
        //SoundManager.Instance.Meth_Shoot_Kill(1);
        DropItems(DropBonus);

        // Deactivate the enemy so it can be returned to the pool.
        gameObject.SetActive(false);
        
        //Reset weak point material
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        // Reset material alpha if needed
        if (runtimeExtraMat != null && runtimeExtraMat.HasProperty("_BaseColor"))
        {
            Color resetColor = extraMatBaseColor;
            resetColor.a = 0f; 
            runtimeExtraMat.SetColor("_BaseColor", resetColor);
        }
    }

    // Drops energy items with a random offset.
    private void DropItems(float DropBonus)
    {
        for (int i = 0; i < energyDropQuantity + DropBonus; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f)
            );

            Vector3 spawnPosition = transform.position + randomOffset;
            Vector3 direction = (spawnPosition - transform.position).normalized;

            S_EnergyPointPoolManager.Instance.QueueEnergyPoint(energyPoint, spawnPosition, direction);
            
        }
    }

    // Clears instance events on disable to avoid multiple subscriptions.
    private void OnDisable()
    {
        OnKilled = null;
    }
}
