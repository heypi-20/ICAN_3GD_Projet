using UnityEngine;
using System;

[RequireComponent(typeof(S_SeedModule))]
[RequireComponent(typeof(S_GrowthFrequencyModule))]
public class S_UnLockGrowthOnSeedIsActive : MonoBehaviour
{
    private S_SeedModule seedModule;
    private S_GrowthFrequencyModule growthFrequencyModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SeedModule et S_GrowthFrequencyModule
        seedModule = GetComponent<S_SeedModule>();
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
    }

    private void OnEnable()
    {
        if (seedModule != null)
        {
            // Écouter l'événement SeedIsActive du module de graine
            seedModule.SeedIsActive += HandleSeedIsActive;
        }
    }

    private void OnDisable()
    {
        if (seedModule != null)
        {
            // Supprimer l'écoute de l'événement SeedIsActive
            seedModule.SeedIsActive -= HandleSeedIsActive;
        }
    }

    private void HandleSeedIsActive()
    {
        Debug.Log("Événement SeedIsActive détecté. Déverrouiller la croissance...");

        if (growthFrequencyModule != null)
        {
            // Appeler la méthode UnlockGrowth du module de fréquence de croissance
            growthFrequencyModule.UnlockGrowth();
        }
        else
        {
            Debug.LogWarning("Le module de fréquence de croissance n'est pas correctement assigné.");
        }
    }
}
