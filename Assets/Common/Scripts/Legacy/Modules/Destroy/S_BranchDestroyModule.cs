using System;
using UnityEngine;
using System.Collections;

public class S_BranchDestroyModule : MonoBehaviour
{
    public float detectionDistance = 5f; // Distance de détection
    private Vector3 growthDirection; // Direction de croissance

    private void Start()
    {
        // Initialiser la direction de croissance en utilisant la rotation actuelle de l'objet
        growthDirection = -transform.forward;
        StartCoroutine(PeriodicDetection()); // Lancer la détection périodique
    }

    private IEnumerator PeriodicDetection()
    {
        while (true)
        {
            // Dessiner une ligne pour visualiser la direction de détection
            Debug.DrawRay(transform.position, growthDirection * detectionDistance, Color.red);

            // Lancer un raycast vers l'arrière pour vérifier s'il y a un autre objet du même type ou un S_SeedModule
            if (!Physics.Raycast(transform.position, growthDirection, out RaycastHit hit, detectionDistance) ||
                (hit.transform.GetComponent<S_BranchDestroyModule>() == null && hit.transform.GetComponent<S_SeedModule>() == null))
            {
                // S'il n'y a pas de branche connectée derrière, se détruire
                Destroy(gameObject);
                yield break; // Arrêter la coroutine après la destruction
            }

            // Attendre un certain temps avant la prochaine détection pour réduire la charge CPU
            yield return null;
        }
    }
}
