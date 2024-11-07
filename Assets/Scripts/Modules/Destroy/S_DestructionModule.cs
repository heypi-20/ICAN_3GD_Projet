using UnityEngine;
using System;
using System.Collections;

public class S_DestructionModule : MonoBehaviour
{
    public bool destroyForever = false; // Détruire l'objet de manière permanente
    public GameObject destructionEffectPrefab; // Prefab de l'effet de destruction (ex. particules)
    public float destructionEffectLifetime = 2f; // Durée de vie de l'effet de destruction
    public bool useEffectLifetimeForTemporary = true; // Utiliser la durée de vie de l'effet uniquement pour la destruction temporaire

    // Déclaration d'un événement public
    public event Action OnDestroyed;

    // Méthode pour détruire l'objet
    public void DestroyObject()
    {
        // Déclenche l'événement de destruction
        OnDestroyed?.Invoke();
        destructionEffect(); // Générer l'effet de destruction

        if (destroyForever)
        {
            // Destruction permanente de l'objet
            Destroy(gameObject);
        }
        else
        {
            // Désactivation temporaire de l'objet
            DisableObject();
        }
    }

    // Méthode pour générer l'effet de destruction
    private void destructionEffect()
    {
        // Génération de l'effet de destruction s'il est spécifié
        if (destructionEffectPrefab != null)
        {
            GameObject effectInstance = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);

            // Gestion de la destruction de l'effet en fonction de la configuration
            if (destroyForever)
            {
                // Destruction permanente - détruire l'effet après un délai
                Destroy(effectInstance, destructionEffectLifetime);
            }
            else
            {
                // Désactivation temporaire - choisir la méthode de destruction de l'effet
                if (useEffectLifetimeForTemporary)
                {
                    // Si la durée de vie de l'effet est activée, détruire après un délai
                    Destroy(effectInstance, destructionEffectLifetime);
                }
                else
                {
                    // Si la durée de vie n'est pas activée, détruire l'effet après la réactivation de l'objet
                    StartCoroutine(DestroyEffectOnReactivation(effectInstance));
                }
            }
        }
    }

    // Méthode pour désactiver l'objet (rendre l'objet "invisible" dans le jeu)
    private void DisableObject()
    {
        // Désactivation des composants visuels, physiques et de collision
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Désactivation d'autres composants si nécessaire
    }

    // Coroutine pour détruire l'effet de destruction lorsque l'objet est réactivé
    private IEnumerator DestroyEffectOnReactivation(GameObject effectInstance)
    {
        yield return null; // Attendre une frame pour s'assurer que l'objet a bien été désactivé
        Renderer renderer = GetComponent<Renderer>();
        while (renderer == null || !renderer.enabled)
        {
            yield return null; // Attendre que le composant visuel de l'objet soit réactivé
        }

        // Détruire l'effet de destruction après la réactivation de l'objet
        Destroy(effectInstance);
    }
}
