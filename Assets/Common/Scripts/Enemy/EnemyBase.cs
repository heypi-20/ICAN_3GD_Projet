using System;
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

    private float currentHealth;
    private bool isDead = false;
    private S_ScoreDisplay _s_ScoreDisplay;

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
            WeakPoint.SetActive(true);
        }

        // Trigger death if health is zero or below.
        if (currentHealth <= 0)
        {
            EnemyDied(DropBonus);
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
