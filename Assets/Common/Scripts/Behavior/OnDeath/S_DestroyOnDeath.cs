using UnityEngine;
using System;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_DestructionModule))]
public class S_DestroyOnDeath : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_DestructionModule destructionModule;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_HealthModule et S_DestructionModule
        healthModule = GetComponent<S_HealthModule>();
        destructionModule = GetComponent<S_DestructionModule>();
    }

    private void OnEnable()
    {
        if (healthModule != null)
        {
            // �couter l'�v�nement OnDeath du module de sant�
            healthModule.OnDeath += HandleOnDeath;
        }
    }

    private void OnDisable()
    {
        if (healthModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnDeath
            healthModule.OnDeath -= HandleOnDeath;
        }
    }

    private void HandleOnDeath()
    {

        if (destructionModule != null)
        {
            // Appeler la m�thode DestroyObject du module de destruction
            destructionModule.DestroyObject();
        }
        else
        {
            Debug.LogWarning("Le module de destruction n'est pas correctement assign�.");
        }
    }
}
