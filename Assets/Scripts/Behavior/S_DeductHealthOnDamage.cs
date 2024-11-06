using UnityEngine;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_DamageModule))]
public class S_DeductHealthOnDamage : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_DamageModule damageModule;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_HealthModule et S_DamageModule
        healthModule = GetComponent<S_HealthModule>();
        damageModule = GetComponent<S_DamageModule>();
    }

    private void OnEnable()
    {
        if (damageModule != null)
        {
            // �couter l'�v�nement OnDamage du module de d�g�ts
            damageModule.OnDamage += HandleOnDamage;
        }
    }

    private void OnDisable()
    {
        if (damageModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnDamage
            damageModule.OnDamage -= HandleOnDamage;
        }
    }

    private void HandleOnDamage(float damageAmount)
    {
        Debug.Log("OnDamage event detected. Deducting health...");

        if (healthModule != null)
        {
            // Appeler la m�thode ApplyDamage de HealthModule avec la valeur de d�g�ts re�ue
            healthModule.ApplyDamage(damageAmount);
        }
        else
        {
            Debug.LogWarning("Health module is not correctly assigned.");
        }
    }
}
