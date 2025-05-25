using System;
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
    private Coroutine graceCoroutine;

    public event Action<Enum> OnPlayerHealthState;

    private void Start()
    {
        // Cache reference for performance
        _energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void Update()
    {
        if (_energyStorage == null)
            return;

        // If player has entered one-hit mode but regained energy, reset modes
        if (oneHitMode && _energyStorage.currentEnergy > 0f)
        {
            ResetOneHitKillMode();
        }

        // Check energy depletion outside of damage events
        if (!oneHitMode && !_energyStorage.isGraceActive && !isDead && _energyStorage.currentEnergy <= 0f)
        {
            ActivateOneHitKillMode();
        }
    }

    private void OnPlayerHealthStateChange(Enum state)
    {
        OnPlayerHealthState?.Invoke(state);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_energyStorage == null) return;

        // Only one method handles damage now
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            ReceiveDamage(enemy.enemyDamage);
        }
    }

    // Called by triggers or other scripts to apply damage
    public void ReceiveDamage(float damage)
    {
        if (isDead)
            return;

        if (oneHitMode)
        {
            if (isInvulnerable)
                // Ignore hits during local grace period
                return;

            // Next hit kills
            isDead = true;
            HandleDeath();
            return;
        }

        // Normal damage: remove energy
        OnPlayerHealthStateChange(PlayerStates.PlayerHealthState.PlayerGetHit);
        _energyStorage.RemoveEnergy(damage);

        // Check energy and activate one–hit mode if depleted
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
        OnPlayerHealthStateChange(PlayerStates.PlayerHealthState.PlayerOneHitModeStart);

        // Start timer for invulnerability
        graceCoroutine = StartCoroutine(GracePeriodCoroutine());
    }

    private IEnumerator GracePeriodCoroutine()
    {
        // Allow player time to react
        Debug.Log("Grace period started");
        yield return new WaitForSeconds(graceDuration);

        // End invulnerability so next damage will kill
        isInvulnerable = false;

        // Clear coroutine reference
        graceCoroutine = null;
    }

    /// <summary>
    /// Resets one-hit kill and invulnerability states when energy is restored
    /// </summary>
    private void ResetOneHitKillMode()
    {
        // Stop any running grace coroutine
        if (graceCoroutine != null)
        {
            StopCoroutine(graceCoroutine);
            graceCoroutine = null;
        }

        oneHitMode = false;
        isInvulnerable = false;
        OnPlayerHealthStateChange(PlayerStates.PlayerHealthState.PlayerOneHitModeEnd);
    }

    private void HandleDeath()
    {
        // Death logic (animations, UI, respawn) goes here
        FindObjectOfType<S_GameResultCalcul>().ShowResultScreen();

    }
}
