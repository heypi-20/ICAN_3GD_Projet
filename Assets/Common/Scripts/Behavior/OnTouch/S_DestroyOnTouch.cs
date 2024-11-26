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
        // Ajouter automatiquement et r�f�rencer S_PhysicsCollisionModule et S_DestructionModule
        physicsCollisionModule = GetComponent<S_PhysicsCollisionModule>();
        destructionModule = GetComponent<S_DestructionModule>();
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
