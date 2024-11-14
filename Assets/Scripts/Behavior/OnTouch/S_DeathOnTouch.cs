using UnityEngine;

[RequireComponent(typeof(S_HealthModule))]
[RequireComponent(typeof(S_PhysicsCollisionModule))]
public class S_DeathOnTouch : MonoBehaviour
{
    private S_HealthModule healthModule;
    private S_PhysicsCollisionModule collisionModule;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_HealthModule et S_PhysicsCollisionModule
        healthModule = GetComponent<S_HealthModule>();
        collisionModule = GetComponent<S_PhysicsCollisionModule>();
    }

    private void OnEnable()
    {
        if (collisionModule != null)
        {
            // �couter l'�v�nement OnTouch du module de collision physique
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

        if (healthModule != null)
        {
            // Appeler la m�thode Die de HealthModule
            healthModule.Die();
        }
        else
        {
            Debug.LogWarning("Health module is not correctly assigned.");
        }
    }
}
