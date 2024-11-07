using UnityEngine;
using System;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_DroppingModule))]
public class S_DropOnDeath : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_DroppingModule droppingModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_HealthModule et S_DroppingModule
        healthModule = GetComponent<S_HealthModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (healthModule != null)
        {
            // Écouter l'événement OnDeath du module de santé
            healthModule.OnDeath += HandleOnDeath;
        }
    }

    private void OnDisable()
    {
        if (healthModule != null)
        {
            // Supprimer l'écoute de l'événement OnDeath
            healthModule.OnDeath -= HandleOnDeath;
        }
    }

    private void HandleOnDeath()
    {
        Debug.Log("Événement OnDeath détecté. Lâcher des objets...");

        if (droppingModule != null)
        {
            // Appeler la méthode DropItems du module de largage
            droppingModule.DropItems();
        }
        else
        {
            Debug.LogWarning("Le module de largage n'est pas correctement assigné.");
        }
    }
}
