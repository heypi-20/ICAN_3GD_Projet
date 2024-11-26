using UnityEngine;
using System;

public class S_HealthModule : MonoBehaviour
{
    public float currentHealth = 100f;  // Santé actuelle
    public float maxHealthLimit = 100f;  // Limite maximale de la santé

    public event Action OnDeath;  // Événement déclenché lorsque l'objet meurt

    private void Start()
    {
        // Limiter la santé actuelle entre 0 et la limite maximale
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthLimit);
    }

    public void ApplyDamage(float damageAmount)
    {
        // Réduire la santé actuelle du montant des dégâts reçus
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthLimit);

        // Si la santé atteint 0, déclencher la mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();  // Déclencher l'événement OnDeath si des abonnés sont présents
    }
}
