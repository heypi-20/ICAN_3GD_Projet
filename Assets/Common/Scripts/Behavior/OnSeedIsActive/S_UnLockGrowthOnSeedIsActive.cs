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
        // Ajouter automatiquement et r�f�rencer S_SeedModule et S_GrowthFrequencyModule
        seedModule = GetComponent<S_SeedModule>();
        growthFrequencyModule = GetComponent<S_GrowthFrequencyModule>();
    }

    private void OnEnable()
    {
        if (seedModule != null)
        {
            // �couter l'�v�nement SeedIsActive du module de graine
            seedModule.SeedIsActive += HandleSeedIsActive;
        }
    }

    private void OnDisable()
    {
        if (seedModule != null)
        {
            // Supprimer l'�coute de l'�v�nement SeedIsActive
            seedModule.SeedIsActive -= HandleSeedIsActive;
        }
    }

    private void HandleSeedIsActive()
    {
        Debug.Log("�v�nement SeedIsActive d�tect�. D�verrouiller la croissance...");

        if (growthFrequencyModule != null)
        {
            // Appeler la m�thode UnlockGrowth du module de fr�quence de croissance
            growthFrequencyModule.UnlockGrowth();
        }
        else
        {
            Debug.LogWarning("Le module de fr�quence de croissance n'est pas correctement assign�.");
        }
    }
}
