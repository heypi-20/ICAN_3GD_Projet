using UnityEngine;
using System;

public class S_HealthModule : MonoBehaviour
{
    public float currentHealth = 100f;  // Sant� actuelle
    public float maxHealthLimit = 100f;  // Limite maximale de la sant�

    public event Action OnDeath;  // �v�nement d�clench� lorsque l'objet meurt

    private void Start()
    {
        // Limiter la sant� actuelle entre 0 et la limite maximale
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthLimit);
    }

    public void ApplyDamage(float damageAmount)
    {
        // R�duire la sant� actuelle du montant des d�g�ts re�us
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealthLimit);

        // Si la sant� atteint 0, d�clencher la mort
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();  // D�clencher l'�v�nement OnDeath si des abonn�s sont pr�sents
    }
}
