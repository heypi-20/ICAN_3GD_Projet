using UnityEngine;

[RequireComponent(typeof(S_PhysicsCollisionModule))]
[RequireComponent(typeof(S_DamageModule))]
public class S_DamageOnTouch : MonoBehaviour
{
    [Header("Damage Settings")]
    public float baseDamage = 10f;  // Valeur de dégâts de base, peut être modifiée par d'autres scripts

    private S_PhysicsCollisionModule collisionModule;
    private S_DamageModule damageModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PhysicsCollisionModule et S_DamageModule
        collisionModule = GetComponent<S_PhysicsCollisionModule>();
        damageModule = GetComponent<S_DamageModule>();
    }

    private void OnEnable()
    {
        if (collisionModule != null)
        {
            // Écouter l'événement OnTouch du module de collision physique
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
        Debug.Log("OnTouch event detected. Applying damage...");

        if (damageModule != null)
        {
            // Appeler la méthode ReceiveDamage de DamageModule avec la valeur de dégâts de base
            damageModule.ReceiveDamage(baseDamage);
        }
        else
        {
            Debug.LogWarning("Damage module is not correctly assigned.");
        }
    }
}
