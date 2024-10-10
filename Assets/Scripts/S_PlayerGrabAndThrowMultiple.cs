using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerGrabAndThrowMultiple : MonoBehaviour
{
    [Header("Grabbing Settings")]
    [Tooltip("Taille de la zone de prise, ajustable pour la largeur, la hauteur et la profondeur")]
    public Vector3 grabBoxSize = new Vector3(1f, 1f, 1.5f);
    [Tooltip("Distance entre le joueur et la zone de prise")]
    public float grabBoxDistance = 1f;
    [Tooltip("Le nombre maximum d'objets que le joueur peut attraper")]
    public int maxGrabCount = 5;  // 限制最多抓取的数量
    [Tooltip("La force appliquée lors du lancer")]
    public float throwForce = 10f;
    [Tooltip("L'angle à appliquer pour ajouter un arc lors du lancer (degrés)")]
    public float throwAngle = 45f;

    [Header("Keybinds")]
    [Tooltip("La touche utilisée pour attraper")]
    public KeyCode grabKey = KeyCode.E;
    [Tooltip("La touche utilisée pour lancer un par un")]
    public KeyCode throwKey = KeyCode.F;  // 用来一个一个扔
    [Tooltip("La touche utilisée pour lancer tous les objets d'un coup")]
    public KeyCode throwAllKey = KeyCode.G;  // 用来一次性全部扔

    [Header("Catch Point Settings")]
    [Tooltip("Le point où l'objet attrapé suivra")]
    public Transform catchPoint; // Le point de la scène où les objets attrapés seront attachés
    [Tooltip("Si vrai, utiliser la même touche pour lancer un par un ou tous les objets")]
    public bool toggleThrowMode = true; // 如果为true,使用同一个按键切换扔一个或全部
    [Tooltip("Cette option nécessite que 'toggleThrowMode' soit activé")]
    public bool throwOneByOne = true;  // 决定是否一个一个扔
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
                obj.transform.position = catchPoint.position;
                obj.transform.rotation = catchPoint.rotation;
            }
        }
    }

    // Essayer d'attraper un objet
    private void TryGrabObject()
    {
        if (grabbedObjects.Count >= maxGrabCount)
        {
            Debug.Log("Maximum grab limit reached.");
            return;
        }

        RaycastHit hit;
        // Utilisation de BoxCast pour détecter les objets avec une taille et distance ajustables
        if (Physics.BoxCast(transform.position, grabBoxSize / 2, transform.forward, out hit, transform.rotation, grabBoxDistance))
        {
            if (hit.collider != null && hit.collider.GetComponent<Rigidbody>() != null && !hit.collider.GetComponent<Rigidbody>().isKinematic)
            {
                Rigidbody grabbedObject = hit.collider.GetComponent<Rigidbody>();
                grabbedObject.GetComponent<Collider>().enabled = false;  // Désactiver le collider pendant l'attrape
                grabbedObject.useGravity = false;
                grabbedObjects.Add(grabbedObject);  // Ajouter à la liste des objets attrapés
            }
        }
    }

    // Lancer un seul objet
    private void ThrowObject()
    {
        if (grabbedObjects.Count > 0)
        {
            Rigidbody objToThrow = grabbedObjects[0];
            grabbedObjects.RemoveAt(0);  // Enlever l'objet de la liste

            // Lancer l'objet
            Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward;
            objToThrow.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            objToThrow.GetComponent<Collider>().enabled = true;  // Réactiver le collider
            objToThrow.useGravity = true;
        }
    }

    // Lancer tous les objets
    private void ThrowAllObjects()
    {
        foreach (var obj in grabbedObjects)
        {
            Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, transform.right) * transform.forward;
            obj.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            obj.GetComponent<Collider>().enabled = true;  // Réactiver le collider
            obj.useGravity = true;
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
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();
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
