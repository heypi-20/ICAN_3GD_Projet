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
        // Ajouter automatiquement et r�f�rencer S_SeedRevertModule et S_SeedModule
        seedRevertModule = GetComponent<S_SeedRevertModule>();
        seedModule = GetComponent<S_SeedModule>();
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
        Debug.Log("�v�nement OnRevertToSeed d�tect�. D�sactiver la graine...");

        if (seedModule != null)
        {
            // Appeler la m�thode DeactivateSeed du module de graine
            seedModule.DeactivateSeed();
        }
        else
        {
            Debug.LogWarning("Le module de graine n'est pas correctement assign�.");
        }
    }
}
