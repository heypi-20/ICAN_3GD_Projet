using System;
using UnityEngine;
using System.Collections;

public class S_BranchDestroyModule : MonoBehaviour
{
    public float detectionDistance = 5f; // Distance de d�tection
    private Vector3 growthDirection; // Direction de croissance

    private void Start()
    {
        // Initialiser la direction de croissance en utilisant la rotation actuelle de l'objet
        growthDirection = -transform.forward;
        StartCoroutine(PeriodicDetection()); // Lancer la d�tection p�riodique
    }

    private IEnumerator PeriodicDetection()
    {
        while (true)
        {
            // Dessiner une ligne pour visualiser la direction de d�tection
            Debug.DrawRay(transform.position, growthDirection * detectionDistance, Color.red);

            // Lancer un raycast vers l'arri�re pour v�rifier s'il y a un autre objet du m�me type ou un S_SeedModule
            if (!Physics.Raycast(transform.position, growthDirection, out RaycastHit hit, detectionDistance) ||
                (hit.transform.GetComponent<S_BranchDestroyModule>() == null && hit.transform.GetComponent<S_SeedModule>() == null))
            {
                // S'il n'y a pas de branche connect�e derri�re, se d�truire
                Destroy(gameObject);
                yield break; // Arr�ter la coroutine apr�s la destruction
            }

            // Attendre un certain temps avant la prochaine d�tection pour r�duire la charge CPU
            yield return null;
        }
    }
}
