using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_PlayerGrabAndThrowMultiple : MonoBehaviour
{
    [Header("Grabbing Settings")]
    [Tooltip("Taille de la zone de prise, ajustable pour la largeur, la hauteur et la profondeur")]
    public Vector3 grabBoxSize = new Vector3(1f, 1f, 1.5f);
    [Tooltip("Distance entre le joueur et la zone de prise")]
    public float grabBoxDistance = 1f;
    [Tooltip("Le nombre maximum d'objets que le joueur peut attraper")]
    public int maxGrabCount = 5;
    [Tooltip("La force appliquée lors du lancer")]
    public float throwForce = 10f;
    [Tooltip("L'angle à appliquer pour ajouter un arc lors du lancer (degrés)")]
    public float throwAngle = 45f;

    [Header("Keybinds")]
    [Tooltip("La touche utilisée pour attraper")]
    public KeyCode grabKey = KeyCode.E;
    [Tooltip("La touche utilisée pour lancer un par un")]
    public KeyCode throwKey = KeyCode.F;
    [Tooltip("La touche utilisée pour lancer tous les objets d'un coup")]
    public KeyCode throwAllKey = KeyCode.G;

    [Header("Catch Point Settings")]
    [Tooltip("Le point où l'objet attrapé suivra")]
    public Transform catchPoint;
    [Tooltip("Si vrai, utiliser la même touche pour lancer un par un ou tous les objets")]
    public bool toggleThrowMode = true;
    [Tooltip("Cette option nécessite que 'toggleThrowMode' soit activée")]
    public bool throwOneByOne = true;
    private List<Rigidbody> grabbedObjects = new List<Rigidbody>(); // Liste des objets attrapés

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            TryGrabObject();  // Tenter d'attraper un objet
        }

        if (toggleThrowMode)
        {
            if (Input.GetKeyDown(throwKey))
            {
                if (throwOneByOne)
                    ThrowObject();  // Lancer un seul objet
                else
                    ThrowAllObjects();  // Lancer tous les objets
            }
        }
        else
        {
            if (Input.GetKeyDown(throwKey))
            {
                ThrowObject();  // Lancer un seul objet
            }
            if (Input.GetKeyDown(throwAllKey))
            {
                ThrowAllObjects();  // Lancer tous les objets
            }
        }

        // Si des objets sont attrapés, ils suivent le catchPoint
        if (grabbedObjects.Count > 0)
        {
            foreach (var obj in grabbedObjects)
            {
                obj.MovePosition(catchPoint.position); // Mettre l'objet à la position du catchPoint
                obj.MoveRotation(catchPoint.rotation); // Ajuster la rotation de l'objet à celle du catchPoint
            }
        }
    }

    // Essayer d'attraper un objet
    private void TryGrabObject()
    {
        if (grabbedObjects.Count >= maxGrabCount)
        {
            Debug.Log("Maximum grab limit reached."); // Afficher un message quand la limite est atteinte
            return;
        }

        RaycastHit hit;
        // Utilisation de BoxCast pour détecter les objets avec une taille et distance ajustables
        if (Physics.BoxCast(transform.position, grabBoxSize / 2, transform.forward, out hit, transform.rotation, grabBoxDistance))
        {
            if (hit.collider != null && hit.collider.GetComponent<Rigidbody>() != null && !hit.collider.GetComponent<Rigidbody>().isKinematic)
            {
                Rigidbody grabbedObject = hit.collider.attachedRigidbody; // Récupérer le Rigidbody de l'objet
                grabbedObject.GetComponent<Collider>().enabled = false;  // Désactiver le collider pendant l'attrape
                grabbedObject.useGravity = false; // Désactiver la gravité
                grabbedObjects.Add(grabbedObject);  // Ajouter à la liste des objets attrapés
            }
        }
    }

    // Lancer un seul objet
    private void ThrowObject()
    {
        if (grabbedObjects.Count > 0)
        {
            Rigidbody objToThrow = grabbedObjects[0]; // Récupérer le premier objet attrapé
            grabbedObjects.RemoveAt(0);  // Enlever l'objet de la liste

            // Lancer l'objet
            Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward; // Calculer la direction du lancer avec un angle
            objToThrow.AddForce(throwDirection * throwForce, ForceMode.Impulse); // Appliquer la force de lancer
            objToThrow.GetComponent<Collider>().enabled = true;  // Réactiver le collider
            if (objToThrow.GetComponent<ThrownByThePlayer>() == null && objToThrow.GetComponent<S_RemoveComponent>() != null)
            {
                objToThrow.AddComponent<ThrownByThePlayer>();// Ajouter un composant indiquant que l'objet a été lancé
            } 
            objToThrow.useGravity = true; // Réactiver la gravité
        }
    }

    // Lancer tous les objets
    private void ThrowAllObjects()
    {
        foreach (var obj in grabbedObjects)
        {
            Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward; // Calculer la direction du lancer avec un angle
            obj.AddForce(throwDirection * throwForce, ForceMode.Impulse); // Appliquer la force de lancer
            obj.GetComponent<Collider>().enabled = true;  // Réactiver le collider
            if (obj.GetComponent<ThrownByThePlayer>() == null)
            {
                obj.AddComponent<ThrownByThePlayer>();
            }
            obj.useGravity = true; // Réactiver la gravité
        }
        grabbedObjects.Clear();  // Vider la liste des objets attrapés après les avoir tous lancés
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
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>(); // Récupérer le Rigidbody de l'objet touché
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
        Gizmos.DrawWireCube(Vector3.zero, grabBoxSize); // Dessiner le cube pour visualiser la zone de BoxCast
    }
}