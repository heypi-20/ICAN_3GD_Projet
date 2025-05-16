using UnityEngine;

[RequireComponent(typeof(Collider))]               // le Collider doit être  IsTrigger = true
public class ProjectileImpactSpawner : MonoBehaviour
{
    /*------------- Réglages dans l’Inspector ----------------------*/
    [Header("Impact FX")]
    [SerializeField] GameObject impactPrefab;      // préfab d’impact à instancier

    [Header("Layers considérés comme sol / décor")]
    [SerializeField] LayerMask groundMask;         // Ground, Décors, etc.

    /*------------- Caches optionnels pour couper la traîne --------*/
    TrailRenderer[] trails;

    /*--------------------------------------------------------------*/
    void Awake()
    {
        trails = GetComponentsInChildren<TrailRenderer>(true);

        /* Pour recevoir OnTriggerEnter :
         *   • Collider.isTrigger = true
         *   • Rigidbody facultatif : isKinematic = true si tu déplaces l’objet par script
         */
    }

    /*----------------- Détection de l’impact ----------------------*/
    void OnTriggerEnter(Collider other)
    {
        // 1. on filtre les couches : on ne réagit qu’au sol / décor
        if ((groundMask.value & (1 << other.gameObject.layer)) == 0) return;

        // 2. point de contact le plus proche
        Vector3 hitPos = other.ClosestPoint(transform.position);

        // 3. normale approximative (du point vers le projectile)
        Vector3 normal = (transform.position - hitPos).normalized;
        if (normal == Vector3.zero)   // au cas où ClosestPoint retourne la même pos
            normal = other.transform.up;

        // 4. instancie le préfab d’impact
        if (impactPrefab != null)
        {
            Quaternion rot = Quaternion.LookRotation(normal); // Z+ vers la normale
            Instantiate(impactPrefab, hitPos, rot);
        }

        // 5. (optionnel) coupe la traîne pour qu’elle se termine proprement
        foreach (var t in trails)
            t.emitting = false;

        // 6. détruit (ou retourne au pool) le projectile après un petit délai
        Destroy(gameObject, 0.15f);
    }
}
