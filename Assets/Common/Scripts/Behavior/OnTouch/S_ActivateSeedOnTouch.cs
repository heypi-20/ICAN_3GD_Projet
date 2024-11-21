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
        // Ajouter automatiquement et r�f�rencer S_PhysicsCollisionModule et S_SeedModule
        physicsCollisionModule = GetComponent<S_PhysicsCollisionModule>();
        seedModule = GetComponent<S_SeedModule>();
    }

    private void OnEnable()
    {
        if (physicsCollisionModule != null)
        {
            // �couter l'�v�nement OnTouch du module de collision physique
            physicsCollisionModule.OnTouch += HandleTouch;
        }
    }

    private void OnDisable()
    {
        if (physicsCollisionModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnTouch
            physicsCollisionModule.OnTouch -= HandleTouch;
        }
    }

    private void HandleTouch()
    {
        Debug.Log("�v�nement OnTouch d�tect�. Activer la graine...");

        if (seedModule != null)
        {
            // Appeler la m�thode ActivateSeed du module de graine
            seedModule.ActivateSeed();
        }
        else
        {
            Debug.LogWarning("Le module de graine n'est pas correctement assign�.");
        }
    }
}
