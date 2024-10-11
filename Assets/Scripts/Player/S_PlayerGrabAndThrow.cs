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

    private Rigidbody grabbedObject = null;
    private bool isGrabbing = false;  // Bool pour suivre si le joueur est en train de tenir un objet

    void Update()
    {
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
        RaycastHit hit;

        // Utilisation de BoxCast pour détecter les objets avec une taille et distance ajustables
        if (Physics.BoxCast(transform.position, grabBoxSize / 2, transform.forward, out hit, transform.rotation, grabBoxDistance))
        {
            if (hit.collider != null && hit.collider.GetComponent<Rigidbody>() != null&& !hit.collider.GetComponent<Rigidbody>().isKinematic)
            {
                grabbedObject = hit.collider.attachedRigidbody;
                grabbedObject.GetComponent<Collider>().enabled = false;  // Désactiver le collider pendant l'attrape
                grabbedObject.useGravity = false;
                isGrabbing = true;  // Marquer que l'objet est maintenant attrapé
            }
        }
    }

    // Relâcher et lancer l'objet
    private void ThrowObject()
    {
        // Calculer la direction du lancer en ajoutant un angle pour créer une trajectoire en arc
        Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward;
        // Appliquer une force pour lancer l'objet
        float adjustedThrowForce = throwForce / grabbedObject.mass;
        grabbedObject.AddForce(throwDirection * adjustedThrowForce, ForceMode.Impulse);
        grabbedObject.GetComponent<Collider>().enabled = true;  // Réactiver le collider
        grabbedObject.useGravity = true;
        if (grabbedObject.GetComponent<ThrownByThePlayer>() == null)
        {
            grabbedObject.AddComponent<ThrownByThePlayer>();
        }
        grabbedObject = null;  // Remettre à zéro l'objet attrapé
        isGrabbing = false;  // Le joueur ne tient plus d'objet
    }

    // Visualiser la zone de BoxCast pour l'attrape
    private void OnDrawGizmos()
    {
        RaycastHit hit;

        // Effectuer un BoxCast pour vérifier s'il y a un objet dans la zone de prise
        bool isHit = Physics.BoxCast(transform.position, grabBoxSize / 2, transform.forward, out hit, transform.rotation, grabBoxDistance);

        // Vérifier si l'objet touché a un Rigidbody et n'est pas cinématique (donc grabable)
        if (isHit && hit.collider != null)
        {
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();
            // Vérifier que l'objet a un Rigidbody et qu'il n'est pas isKinematic, donc grabable
            if (hitRigidbody != null && !hitRigidbody.isKinematic)
            {
                Gizmos.color = Color.red; // Si l'objet est grabable, changer la couleur en rouge
            }
            else
            {
                Gizmos.color = Color.yellow; // Sinon, laisser la couleur en jaune
            }
        }
        else
        {
            Gizmos.color = Color.yellow; // Aucun objet détecté
        }

        // Afficher la zone de BoxCast pour attraper des objets, ajustée à la rotation du joueur
        Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * (grabBoxDistance / 2), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, grabBoxSize);
    }

}
