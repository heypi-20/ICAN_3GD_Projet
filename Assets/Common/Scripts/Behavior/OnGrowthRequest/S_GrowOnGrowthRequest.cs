using UnityEngine;
using System;

[RequireComponent(typeof(S_GrowthFrequencyModule))]
[RequireComponent(typeof(S_GrowthModeModule))]
public class S_GrowOnGrowthRequest : MonoBehaviour
{
    private S_GrowthFrequencyModule growthFrequencyModule;
    private S_GrowthModeModule growthModeModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_GrowthFrequencyModule et S_GrowthModeModule
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
        growthModeModule = GetComponent<S_GrowthModeModule>();
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
        

        if (growthModeModule != null)
        {
            // Appeler la méthode Grow du module de mode de croissance
            growthModeModule.Grow();
        }
        else
        {
            Debug.LogWarning("Le module de mode de croissance n'est pas correctement assigné.");
        }
    }
}
