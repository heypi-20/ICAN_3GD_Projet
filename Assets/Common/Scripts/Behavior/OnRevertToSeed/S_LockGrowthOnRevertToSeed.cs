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
        // Ajouter automatiquement et r�f�rencer S_SeedRevertModule et S_GrowthFrequencyModule
        seedRevertModule = GetComponent<S_SeedRevertModule>();
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
    }

    private void OnEnable()
    {
        if (seedRevertModule != null)
        {
            // �couter l'�v�nement OnRevertToSeed du module de r�version de graine
            seedRevertModule.OnRevertToSeed += HandleRevertToSeed;
        }
    }

    private void OnDisable()
    {
        if (seedRevertModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnRevertToSeed
            seedRevertModule.OnRevertToSeed -= HandleRevertToSeed;
        }
    }

    private void HandleRevertToSeed()
    {

        if (growthFrequencyModule != null)
        {
            // Appeler la m�thode LockGrowth du module de fr�quence de croissance
            growthFrequencyModule.LockGrowth();
        }
        else
        {
            Debug.LogWarning("Le module de fr�quence de croissance n'est pas correctement assign�.");
        }
    }
}
