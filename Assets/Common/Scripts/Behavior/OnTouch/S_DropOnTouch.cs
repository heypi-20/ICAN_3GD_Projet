using UnityEngine;
using System;

[RequireComponent(typeof(S_PhysicsCollisionModule))]
[RequireComponent(typeof(S_DroppingModule))]
public class S_DropOnTouch : MonoBehaviour
{
    private S_PhysicsCollisionModule collisionModule;
    private S_DroppingModule droppingModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PhysicsCollisionModule et S_DroppingModule
        collisionModule = GetComponent<S_PhysicsCollisionModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (collisionModule != null)
        {
            // Écouter l'événement OnTouch du module de collision
            collisionModule.OnTouch += HandleOnTouch;
        }
    }

    private void OnDisable()
    {
        if (collisionModule != null)
        {
            // Supprimer l'écoute de l'événement OnTouch
            collisionModule.OnTouch -= HandleOnTouch;
        }
    }

    private void HandleOnTouch()
    {
        Debug.Log("Événement OnTouch détecté. Lâcher des objets...");

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
