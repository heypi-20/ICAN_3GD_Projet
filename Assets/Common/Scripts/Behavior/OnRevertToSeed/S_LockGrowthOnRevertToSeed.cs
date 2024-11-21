using UnityEngine;
using System;

[RequireComponent(typeof(S_SeedRevertModule))]
[RequireComponent(typeof(S_GrowthFrequencyModule))]
public class S_LockGrowthOnRevertToSeed : MonoBehaviour
{
    private S_SeedRevertModule seedRevertModule;
    private S_GrowthFrequencyModule growthFrequencyModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SeedRevertModule et S_GrowthFrequencyModule
        seedRevertModule = GetComponent<S_SeedRevertModule>();
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
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

        if (growthFrequencyModule != null)
        {
            // Appeler la méthode LockGrowth du module de fréquence de croissance
            growthFrequencyModule.LockGrowth();
        }
        else
        {
            Debug.LogWarning("Le module de fréquence de croissance n'est pas correctement assigné.");
        }
    }
}
