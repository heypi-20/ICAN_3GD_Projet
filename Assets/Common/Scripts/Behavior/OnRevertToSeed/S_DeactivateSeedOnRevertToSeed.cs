using UnityEngine;
using System;

[RequireComponent(typeof(S_SeedRevertModule))]
[RequireComponent(typeof(S_SeedModule))]
public class S_DeactivateSeedOnRevertToSeed : MonoBehaviour
{
    private S_SeedRevertModule seedRevertModule;
    private S_SeedModule seedModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SeedRevertModule et S_SeedModule
        seedRevertModule = GetComponent<S_SeedRevertModule>();
        seedModule = GetComponent<S_SeedModule>();
    }

    private void OnEnable()
    {
        if (seedRevertModule != null)
        {
            // Écouter l'événement OnRevertToSeed du module de réversion de graine
            seedRevertModule.OnRevertToSeed += HandleRevertToSeed;
        }
    }

    private void OnDisable()
    {
        if (seedRevertModule != null)
        {
            // Supprimer l'écoute de l'événement OnRevertToSeed
            seedRevertModule.OnRevertToSeed -= HandleRevertToSeed;
        }
    }

    private void HandleRevertToSeed()
    {
        Debug.Log("Événement OnRevertToSeed détecté. Désactiver la graine...");

        if (seedModule != null)
        {
            // Appeler la méthode DeactivateSeed du module de graine
            seedModule.DeactivateSeed();
        }
        else
        {
            Debug.LogWarning("Le module de graine n'est pas correctement assigné.");
        }
    }
}
