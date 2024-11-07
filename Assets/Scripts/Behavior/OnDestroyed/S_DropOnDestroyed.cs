using UnityEngine;
using System;

[RequireComponent(typeof(S_DestructionModule))]
[RequireComponent(typeof(S_DroppingModule))]
public class S_DropOnDestroyed : MonoBehaviour
{
    private S_DestructionModule destructionModule;
    private S_DroppingModule droppingModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_DestructionModule et S_DroppingModule
        destructionModule = GetComponent<S_DestructionModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (destructionModule != null)
        {
            // Écouter l'événement OnDestroyed du module de destruction
            destructionModule.OnDestroyed += HandleOnDestroyed;
        }
    }

    private void OnDisable()
    {
        if (destructionModule != null)
        {
            // Supprimer l'écoute de l'événement OnDestroyed
            destructionModule.OnDestroyed -= HandleOnDestroyed;
        }
    }

    private void HandleOnDestroyed()
    {
        Debug.Log("Événement OnDestroyed détecté. Lâcher des objets...");

        if (droppingModule != null)
        {
            // Appeler la méthode DropItems du module de largage
            droppingModule.DropItems();
        }
        else
        {
            Debug.LogWarning("Le module de largage n'est pas correctement assigné.");
        }
    }
}
