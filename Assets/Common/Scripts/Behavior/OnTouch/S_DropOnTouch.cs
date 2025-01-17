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
        // Ajouter automatiquement et r�f�rencer S_PhysicsCollisionModule et S_DroppingModule
        collisionModule = GetComponent<S_PhysicsCollisionModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (collisionModule != null)
        {
            // �couter l'�v�nement OnTouch du module de collision
            collisionModule.OnTouch += HandleOnTouch;
        }
    }

    private void OnDisable()
    {
        if (collisionModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnTouch
            collisionModule.OnTouch -= HandleOnTouch;
        }
    }

    private void HandleOnTouch()
    {
        Debug.Log("�v�nement OnTouch d�tect�. L�cher des objets...");

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
