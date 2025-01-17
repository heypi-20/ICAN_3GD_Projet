using UnityEngine;
using System;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_DroppingModule))]
public class S_DropOnDeath : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_DroppingModule droppingModule;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_HealthModule et S_DroppingModule
        healthModule = GetComponent<S_HealthModule>();
        droppingModule = GetComponent<S_DroppingModule>();
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
        Debug.Log("�v�nement OnDeath d�tect�. L�cher des objets...");

        if (droppingModule != null)
        {
            // Appeler la m�thode DropItems du module de largage
            droppingModule.DropItems(0f);
        }
        else
        {
            Debug.LogWarning("Le module de largage n'est pas correctement assign�.");
        }
    }
}
