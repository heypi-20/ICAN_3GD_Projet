using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_PlayerGrabAndThrow : MonoBehaviour
{
    [Header("Grabbing Settings")]
    [Tooltip("Taille de la zone de prise, ajustable pour la largeur, la hauteur et la profondeur")]
    public Vector3 grabBoxSize = new Vector3(1f, 1f, 1.5f);
    [Tooltip("Distance entre le joueur et la zone de prise")]
    public float grabBoxDistance = 1f;
    [Tooltip("La force appliquée lors du lancer")]
    public float throwForce = 10f;
    [Tooltip("L'angle à appliquer pour ajouter un arc lors du lancer (degrés)")]
    public float throwAngle = 45f;

    [Header("Keybinds")]
    [Tooltip("La touche utilisée pour attraper")]
    public KeyCode grabKey = KeyCode.E;
    [Tooltip("La touche utilisée pour lancer")]
    public KeyCode throwKey = KeyCode.F;

    [Header("Catch Point Settings")]
    [Tooltip("Le point où l'objet attrapé suivra")]
    public Transform catchPoint;
    public bool GizmosOn;
    private Rigidbody grabbedObject = null;
    private void RemoveNullReferences()
    {
        if (grabbedObject == null)
        {
            isGrabbing = false;
        }
    }
    private bool isGrabbing = false;  // Bool pour suivre si le joueur est en train de tenir un objet

    void Update()
    {
        RemoveNullReferences();
        if (Input.GetKeyDown(grabKey) && !isGrabbing)
        {
            TryGrabObject();  // Tenter de saisir un objet
        }
        else if (isGrabbing && Input.GetKeyDown(grabKey))
        {
            ThrowObject();  // Si on tient un objet, relâcher l'objet en appuyant sur la touche
        }

        // Si un objet est attrapé, il suit toujours le catchPoint
        if (grabbedObject != null)
        {
            grabbedObject.MovePosition(catchPoint.position);  // Synchroniser la position avec le catchPoint
            grabbedObject.MoveRotation(catchPoint.rotation);  // Synchroniser la rotation avec le catchPoint
        }
    }

    // Essayer d'attraper un objet
    private void TryGrabObject()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position + transform.forward * grabBoxDistance / 2, grabBoxSize / 2, transform.rotation);
        foreach (var collider in colliders)
        {
            if (collider != null && collider.GetComponent<Rigidbody>() != null && !collider.GetComponent<Rigidbody>().isKinematic && collider.GetComponent<S_PlayerGrabAndThrow>()==null)
            {
                grabbedObject = collider.attachedRigidbody;
                grabbedObject.GetComponent<Collider>().enabled = false;  // Désactiver le collider pendant l'attrape
                grabbedObject.useGravity = false;
                if (grabbedObject.GetComponent<CaughtByPlayer>() == null)
                {
                    grabbedObject.AddComponent<CaughtByPlayer>();
                }
                isGrabbing = true;  // Marquer que l'objet est maintenant attrapé
                break; // Attraper uniquement un objet à la fois
            }
        }
    }

    // Relâcher et lancer l'objet
    private void ThrowObject()
    {
        RemoveNullReferences();
        if (grabbedObject == null) return;
        // Calculer la direction du lancer en ajoutant un angle pour créer une trajectoire en arc
        Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward;
        // Appliquer une force pour lancer l'objet
        float adjustedThrowForce = throwForce / grabbedObject.mass;
        grabbedObject.AddForce(throwDirection * adjustedThrowForce, ForceMode.Impulse);
        grabbedObject.GetComponent<Collider>().enabled = true;  // Réactiver le collider
        grabbedObject.useGravity = true;
        if (grabbedObject.GetComponent<CaughtByPlayer>() != null)
        {
            CaughtByPlayer caughtByPlayer = grabbedObject.GetComponent<CaughtByPlayer>();
            Destroy(caughtByPlayer);
        }
        if (grabbedObject.GetComponent<ThrownByThePlayer>() == null && grabbedObject.GetComponent<S_RemoveComponent>() != null)
        {
            grabbedObject.AddComponent<ThrownByThePlayer>();
        }
        grabbedObject = null;  // Remettre à zéro l'objet attrapé
        isGrabbing = false;  // Le joueur ne tient plus d'objet
    }

    // Visualiser la zone de OverlapBox pour l'attrape
    private void OnDrawGizmos()
    {
        if (!GizmosOn) return;
        Collider[] colliders = Physics.OverlapBox(transform.position + transform.forward * grabBoxDistance / 2, grabBoxSize / 2, transform.rotation);
        bool hasGrabbableObjects = false;
        foreach (var collider in colliders)
        {
            if (collider != null && collider.GetComponent<Rigidbody>() != null && !collider.GetComponent<Rigidbody>().isKinematic && collider.GetComponent<S_PlayerGrabAndThrow>() == null)
            {
                hasGrabbableObjects = true;
                break;
            }
        }
        Gizmos.color = hasGrabbableObjects ? Color.red : Color.yellow;

        // Afficher la zone de OverlapBox pour attraper des objets, ajustée à la rotation du joueur
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * (grabBoxDistance / 2), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, grabBoxSize);
    }
}