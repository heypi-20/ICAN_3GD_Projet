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
        // Ajouter automatiquement et r�f�rencer S_GrowthFrequencyModule et S_GrowthModeModule
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
        growthModeModule = GetComponent<S_GrowthModeModule>();
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
        

        if (growthModeModule != null)
        {
            // Appeler la m�thode Grow du module de mode de croissance
            growthModeModule.Grow();
        }
        else
        {
            Debug.LogWarning("Le module de mode de croissance n'est pas correctement assign�.");
        }
    }
}
