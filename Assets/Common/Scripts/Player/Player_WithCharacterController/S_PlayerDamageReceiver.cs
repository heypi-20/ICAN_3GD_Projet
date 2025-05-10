using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_PlayerDamageReceiver : MonoBehaviour
{
    [Header("One–Hit Kill Settings")]
    [Tooltip("Duration of temporary invulnerability after energy reaches zero (in seconds)")]
    public float graceDuration = 5f;

    private S_EnergyStorage _energyStorage;
    private bool oneHitMode;
    private bool isInvulnerable;
    private bool isDead;

    // expose current energy for UI or other systems
    public float CurrentEnergy => _energyStorage != null ? _energyStorage.currentEnergy : 0f;

    private void Start()
    {
        // cache reference for performance
        _energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_energyStorage == null) return;

        // only one method handles damage now
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            ReceiveDamage(enemy.enemyDamage);
        }
        else
        {
            var projectile = other.GetComponent<S_EnemyProjectileDamage>();
            if (projectile != null)
                ReceiveDamage(projectile.damage);
        }
    }

    // called by triggers or other scripts to apply damage
    public void ReceiveDamage(float damage)
    {
        if (isDead)
            return;

        if (oneHitMode)
        {
            if (isInvulnerable)
                // ignore hits during local grace period
                return;

            // local grace over: next hit kills
            isDead = true;
            HandleDeath();
            return;
        }

        // normal damage: remove energy
        _energyStorage.RemoveEnergy(damage);

        // check S_EnergyStorage's external grace flag before activating one–hit mode
        if (_energyStorage.currentEnergy <= 0f && !oneHitMode && !_energyStorage.isGraceActive)
        {
            ActivateOneHitKillMode();
        }
    }

    private void ActivateOneHitKillMode()
    {
        oneHitMode = true;
        isInvulnerable = true;
        Debug.Log("Activate one hit kill mode");
        // start timer for invulnerability
        StartCoroutine(GracePeriodCoroutine());
    }

    private IEnumerator GracePeriodCoroutine()
    {
        // allow player time to react
        Debug.Log("Grace period started");
        yield return new WaitForSeconds(graceDuration);
        // end invulnerability so next damage will kill
        isInvulnerable = false;
    }

    private void HandleDeath()
    {
        // death logic (animations, UI, respawn) goes here
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
