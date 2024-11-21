using UnityEngine;
using System;

[RequireComponent(typeof(S_GrowthFrequencyModule))]
[RequireComponent(typeof(S_SeedRevertModule))]
public class S_StartCheckingChildCountOnGrowthRequest : MonoBehaviour
{
    private S_GrowthFrequencyModule growthFrequencyModule;
    private S_SeedRevertModule seedRevertModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_GrowthFrequencyModule et S_SeedRevertModule
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
        seedRevertModule = GetComponent<S_SeedRevertModule>();
    }

    private void OnEnable()
    {
        if (growthFrequencyModule != null)
        {
            // Écouter l'événement GrowthRequest du module de fréquence de croissance
            growthFrequencyModule.GrowthRequest += HandleGrowthRequest;
        }
    }

    private void OnDisable()
    {
        if (growthFrequencyModule != null)
        {
            // Supprimer l'écoute de l'événement GrowthRequest
            growthFrequencyModule.GrowthRequest -= HandleGrowthRequest;
        }
    }

    private void HandleGrowthRequest()
    {
        if (seedRevertModule != null)
        {
            // Appeler la méthode StartCheckingChildCount du module de réversion de graine
            seedRevertModule.StartCheckingChildCount();
        }
        else
        {
            Debug.LogWarning("Le module de réversion de graine n'est pas correctement assigné.");
        }
    }
}
