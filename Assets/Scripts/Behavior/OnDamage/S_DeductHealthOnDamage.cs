using UnityEngine;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_DamageModule))]
public class S_DeductHealthOnDamage : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_DamageModule damageModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_HealthModule et S_DamageModule
        healthModule = GetComponent<S_HealthModule>();
        damageModule = GetComponent<S_DamageModule>();
    }

    private void OnEnable()
    {
        if (damageModule != null)
        {
            // Écouter l'événement OnDamage du module de dégâts
            damageModule.OnDamage += HandleOnDamage;
        }
    }

    private void OnDisable()
    {
        if (damageModule != null)
        {
            // Supprimer l'écoute de l'événement OnDamage
            damageModule.OnDamage -= HandleOnDamage;
        }
    }

    private void HandleOnDamage(float damageAmount)
    {
        Debug.Log("OnDamage event detected. Deducting health...");

        if (healthModule != null)
        {
            // Appeler la méthode ApplyDamage de HealthModule avec la valeur de dégâts reçue
            healthModule.ApplyDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("Health module is not correctly assigned.");
        }
    }
}
