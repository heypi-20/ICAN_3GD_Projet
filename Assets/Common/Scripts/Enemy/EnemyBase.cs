using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public string enemyName;
    public EnemyType enemyType; // Used for tracking and pooling
    public float health;
    public float WeaknessExposureHealth;
    public float enemyDamage;
    public GameObject WeakPoint;
    public GameObject energyPoint;
    public float energyDropQuantity;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public EventReference enemy_Kill;
    private EventInstance enemy_kill_instance;
    public string enemyDeathSound;
    [Header("WeakPoint Visuals")]
    public Material extraWeakPointMaterial;
    public Renderer weakPointRenderer;
    private Material[] originalWeakPointMaterials;

    [HideInInspector]
    public float currentHealth;
    private bool isDead = false;
    private S_PLayerInfoDebugDisplay _sPLayerInfoDebugDisplay;


    public static event Action<EnemyType> OnEnemyKilled; // Global event for combo tracking
    public event Action<EnemyBase> OnKilled; // Instance event for pooling
    
    // Called when the object is enabled (e.g., when reused from the pool)

    protected virtual void OnEnable()
    {
        FindWeakPoint();             // Find and disable the weak point
        currentHealth = health;      // Reset health
        isDead = false;              // Reset death flag for pooling reuse
        if (weakPointRenderer != null)
        {
            originalWeakPointMaterials = weakPointRenderer.materials;
        }
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
    public void ReduceHealth(float amount, int DropBonus,Vector3 hitPosition=default)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (hitPosition == default)
        {
            hitPosition = transform.position;
        }

        // Show hit effect if available.
        if (enemyGetHitVFX != null)
        {
            S_VFXPoolManager.Instance.SpawnVFX(enemyGetHitVFX, hitPosition, transform.rotation, 3f);

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
            Material[] currentMats = weakPointRenderer.materials;

            foreach (var mat in currentMats)
            {
                if (mat == extraWeakPointMaterial) return;
            }

            Material[] newMats = new Material[currentMats.Length + 1];
            currentMats.CopyTo(newMats, 0);
            newMats[^1] = extraWeakPointMaterial;
            weakPointRenderer.materials = newMats;
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
        OnEnemyKilled?.Invoke(enemyType);
        OnKilled?.Invoke(this);

        // Play kill sound and drop energy items.
        enemy_kill_instance = RuntimeManager.CreateInstance(enemy_Kill);
        RuntimeManager.AttachInstanceToGameObject(enemy_kill_instance,transform,GetComponent<Rigidbody>());
        enemy_kill_instance.start();
        DropItems(DropBonus);

        // Deactivate the enemy so it can be returned to the pool.
        gameObject.SetActive(false);
        RemoveWeakPointMat();
    }

    private void RemoveWeakPointMat()
    {
        // Remove extra material from weak point
        if (weakPointRenderer != null && originalWeakPointMaterials != null)
        {
            weakPointRenderer.materials = originalWeakPointMaterials;
        }
    }
    

    // Drops energy items with a random offset.
    public void DropItems(float DropBonus)
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
