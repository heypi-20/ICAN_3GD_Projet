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
        // Ajouter automatiquement et r�f�rencer S_DestructionModule et S_DroppingModule
        destructionModule = GetComponent<S_DestructionModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (destructionModule != null)
        {
            // �couter l'�v�nement OnDestroyed du module de destruction
            destructionModule.OnDestroyed += HandleOnDestroyed;
        }
    }

    private void OnDisable()
    {
        if (destructionModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnDestroyed
            destructionModule.OnDestroyed -= HandleOnDestroyed;
        }
    }

    private void HandleOnDestroyed()
    {
        Debug.Log("�v�nement OnDestroyed d�tect�. L�cher des objets...");

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
