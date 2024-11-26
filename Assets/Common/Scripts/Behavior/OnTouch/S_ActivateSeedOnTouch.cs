using UnityEngine;
using System;

[RequireComponent(typeof(S_PhysicsCollisionModule))]
[RequireComponent(typeof(S_SeedModule))]
public class S_ActivateSeedOnTouch : MonoBehaviour
{
    private S_PhysicsCollisionModule physicsCollisionModule;
    private S_SeedModule seedModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PhysicsCollisionModule et S_SeedModule
        physicsCollisionModule = GetComponent<S_PhysicsCollisionModule>();
        seedModule = GetComponent<S_SeedModule>();
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
        Debug.Log("Événement OnTouch détecté. Activer la graine...");

        if (seedModule != null)
        {
            // Appeler la méthode ActivateSeed du module de graine
            seedModule.ActivateSeed();
        }
        else
        {
            Debug.LogWarning("Le module de graine n'est pas correctement assigné.");
        }
    }
}
