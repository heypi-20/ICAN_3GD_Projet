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
        // Ajouter automatiquement et r�f�rencer S_GrowthFrequencyModule et S_SeedRevertModule
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
        seedRevertModule = GetComponent<S_SeedRevertModule>();
    }

    private void OnEnable()
    {
        if (growthFrequencyModule != null)
        {
            // �couter l'�v�nement GrowthRequest du module de fr�quence de croissance
            growthFrequencyModule.GrowthRequest += HandleGrowthRequest;
        }
    }

    private void OnDisable()
    {
        if (growthFrequencyModule != null)
        {
            // Supprimer l'�coute de l'�v�nement GrowthRequest
            growthFrequencyModule.GrowthRequest -= HandleGrowthRequest;
        }
    }

    private void HandleGrowthRequest()
    {
        if (seedRevertModule != null)
        {
            // Appeler la m�thode StartCheckingChildCount du module de r�version de graine
            seedRevertModule.StartCheckingChildCount();
        }
        else
        {
            Debug.LogWarning("Le module de r�version de graine n'est pas correctement assign�.");
        }
    }
}
