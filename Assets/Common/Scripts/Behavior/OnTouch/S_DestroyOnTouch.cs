using UnityEngine;
using System;

[RequireComponent(typeof(S_PhysicsCollisionModule))]
[RequireComponent(typeof(S_DestructionModule))]
public class S_DestroyOnTouch : MonoBehaviour
{
    private S_PhysicsCollisionModule physicsCollisionModule;
    private S_DestructionModule destructionModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PhysicsCollisionModule et S_DestructionModule
        physicsCollisionModule = GetComponent<S_PhysicsCollisionModule>();
        destructionModule = GetComponent<S_DestructionModule>();
    }

    private void OnEnable()
    {
        if (physicsCollisionModule != null)
        {
            // Écouter l'événement OnTouch du module de collision physique
            physicsCollisionModule.OnTouch += HandleTouch;
        }
    }

    private void OnDisable()
    {
        if (physicsCollisionModule != null)
        {
            // Supprimer l'écoute de l'événement OnTouch
            physicsCollisionModule.OnTouch -= HandleTouch;
        }
    }

    private void HandleTouch()
    {
        

        if (destructionModule != null)
        {
            // Appeler la méthode DestroyObject du module de destruction
            destructionModule.DestroyObject();
        }
        else
        {
            Debug.LogWarning("Le module de destruction n'est pas correctement assigné.");
        }
    }
}
