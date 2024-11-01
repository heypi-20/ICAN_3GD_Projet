using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DestructibleWall : MonoBehaviour
{
    [Header("Prefab to spawn")]
    [Tooltip("L'effet de particule lors de la destruction du mur")]
    public GameObject destructionEffect;

    [Space(10)]
    [Tooltip("Le prefab � g�n�rer lors de la destruction (effet de d�bris ou encore plus)")]
    public GameObject shatterPrefab;

    [Tooltip("Nombre de pr�fabs � g�n�rer lors de la destruction")]
    public int numberOfShatterPieces = 5;

    [Tooltip("La force appliqu�e aux pr�fabs lors de la destruction")]
    public float shatterForce = 10f;

    [Space(10)]
    [Tooltip("Le prefab suppl�mentaire � g�n�rer lors de la destruction (Neutral Cube par exemple)")]
    public GameObject additionalPrefab;

    [Tooltip("Le nombre de r�initialisations avant de g�n�rer le prefab suppl�mentaire")]
    public int additionalPrefabSpawnRound = 1;

    [Space(20)]
    [Header("Destroy")]
    [Tooltip("D�truire l'objet qui touche le mur")] 
    public bool destroyTouchingObject = false;

    [Tooltip("Tag des objets qui doivent d�truire le mur")] 
    public string DestroyTag = "Growing";

    [Tooltip("Nombre de r�initialisations avant la r�g�n�ration du mur")] 
    public int regrowThreshold = 3;

    [Tooltip("Choix de d�truire ou non les morceaux de d�bris lors de la r�initialisation")] 
    public bool destroyPiecesOnReset = true;

    private int currentResetRound = 0; // Compteur pour suivre le nombre de r�initialisations
    private bool wasDestroyed = false; // Indique si le mur a �t� d�truit
    private Vector3 originalPosition; // Position initiale du mur
    private Quaternion originalRotation; // Rotation initiale du mur
    private Vector3 savedColliderSize; // Taille initiale du collider du mur
    private GameObject activeEffect; // R�f�rence � l'effet de destruction actif
    private List<GameObject> shatterPieces = new List<GameObject>(); // Liste des morceaux de d�bris g�n�r�s lors de la destruction
    private bool additionalPrefabGenerated = false; // Indique si le prefab suppl�mentaire a d�j� �t� g�n�r�

    // S'abonner � l'�v�nement de r�initialisation lors de l'activation de l'objet
    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += ResetDestroyedWall; // Ajouter la m�thode ResetDestroyedWall � l'�v�nement OnZoneReset
    }

    // Se d�sabonner de l'�v�nement de r�initialisation lors de la d�sactivation de l'objet
    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= ResetDestroyedWall; // Retirer la m�thode ResetDestroyedWall de l'�v�nement OnZoneReset
    }

    void Start()
    {
        // Sauvegarder la position, la rotation initiales et la taille du collider du mur
        originalPosition = transform.position; // Sauvegarde de la position initiale
        originalRotation = transform.rotation; // Sauvegarde de la rotation initiale
        Collider collider = GetComponent<Collider>(); // R�cup�rer le collider du mur
        if (collider != null)
        {
            savedColliderSize = collider.bounds.size; // Sauvegarder la taille du collider si elle existe
        }
    }

    void FixedUpdate()
    {
        DetectGrowingObjects(); // Appeler la m�thode pour d�tecter les objets en croissance autour du mur
    }

    void OnCollisionEnter(Collision collision)
    {
        // V�rifier si l'objet en collision a le composant sp�cifique et si le mur n'est pas d�j� d�truit
        if (collision.gameObject.GetComponent<ThrownByThePlayer>() != null && !wasDestroyed)
        {
            DestroyWall(); // D�truire le mur
            // Choix de d�truire l'objet qui touche le mur
            if (destroyTouchingObject)
            {
                Destroy(collision.gameObject); // D�truire l'objet en collision
            }
        }
    }

    // Logique pour d�truire le mur
    private void DestroyWall()
    {
        wasDestroyed = true; // Indiquer que le mur est maintenant d�truit
        additionalPrefabGenerated = false; // R�initialiser le statut du prefab suppl�mentaire

        // D�sactiver le MeshRenderer, Collider et Rigidbody du mur
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>(); // R�cup�rer le MeshRenderer
        Collider collider = GetComponent<Collider>(); // R�cup�rer le collider
        Rigidbody rigidbody = GetComponent<Rigidbody>(); // R�cup�rer le Rigidbody

        if (meshRenderer != null) meshRenderer.enabled = false; // D�sactiver le MeshRenderer si pr�sent
        if (collider != null) collider.enabled = false; // D�sactiver le collider si pr�sent
        if (rigidbody != null) rigidbody.detectCollisions = false; // D�sactiver la d�tection des collisions si Rigidbody pr�sent

        // G�n�rer l'effet de destruction ou les morceaux de d�bris
        if (shatterPrefab != null)
        {
            GenerateShatterPieces(); // Appeler la m�thode pour g�n�rer les morceaux de d�bris
        }
        else if (destructionEffect != null)
        {
            activeEffect = Instantiate(destructionEffect, transform.position, Quaternion.identity); // G�n�rer l'effet de destruction
        }
    }

    // G�n�rer les morceaux de d�bris
    private void GenerateShatterPieces()
    {
        for (int i = 0; i < numberOfShatterPieces; i++)
        {
            GameObject piece = Instantiate(shatterPrefab, transform.position, Random.rotation); // G�n�rer un morceau de d�bris avec une rotation al�atoire
            Rigidbody pieceRb = piece.GetComponent<Rigidbody>(); // R�cup�rer le Rigidbody du morceau de d�bris
            if (pieceRb != null)
            {
                pieceRb.AddExplosionForce(shatterForce, transform.position, 5f); // Ajouter une force d'explosion aux morceaux de d�bris
            }
            shatterPieces.Add(piece); // Ajouter le morceau de d�bris � la liste des morceaux
        }
    }

    // R�initialiser le mur
    public void ResetDestroyedWall()
    {
        // Ne r�initialiser que si le mur a �t� d�truit
        if (GetComponent<MeshRenderer>().enabled)
        {
            Debug.Log("Le mur n'est pas d�truit, pas besoin de r�initialiser."); // Afficher un message dans la console si le mur n'est pas d�truit
            return; // Sortir de la m�thode
        }
        currentResetRound++; // Incr�menter le compteur de r�initialisations

        // G�n�rer un prefab suppl�mentaire apr�s un certain nombre de r�initialisations
        if (wasDestroyed && !additionalPrefabGenerated && currentResetRound >= additionalPrefabSpawnRound && additionalPrefab != null)
        {
            Instantiate(additionalPrefab, transform.position, Quaternion.identity); // G�n�rer le prefab suppl�mentaire
            additionalPrefabGenerated = true; // Indiquer que le prefab suppl�mentaire a �t� g�n�r�
        }

        // V�rifier si des objets avec le tag "DestroyTag" sont pr�sents
        if (AreDestroyTagObjectsPresent())
        {
            Debug.Log("Des objets avec le tag 'DestroyTag' sont toujours pr�sents, r�initialisation du mur report�e."); // Afficher un message dans la console
            return; // Sortir de la m�thode
        }

        // Si le mur doit �tre r�activ� apr�s un certain nombre de r�initialisations
        if (wasDestroyed && currentResetRound >= regrowThreshold)
        {
            // R�activer le MeshRenderer, Collider, et Rigidbody du mur
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Collider collider = GetComponent<Collider>();
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if (meshRenderer != null) meshRenderer.enabled = true; // R�activer le MeshRenderer
            if (collider != null) collider.enabled = true; // R�activer le collider
            if (rigidbody != null) rigidbody.detectCollisions = true; // R�activer la d�tection des collisions du Rigidbody

            // R�initialiser la position et la rotation du mur
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            // D�truire l'effet de destruction
            if (activeEffect != null)
            {
                Destroy(activeEffect); // D�truire l'effet de particule
            }

            // D�truire les morceaux de d�bris si n�cessaire
            if (destroyPiecesOnReset)
            {
                foreach (GameObject piece in shatterPieces)
                {
                    Destroy(piece); // D�truire chaque morceau de d�bris
                }
                shatterPieces.Clear(); // Vider la liste des morceaux de d�bris
            }

            // R�initialiser le compteur de r�initialisations
            currentResetRound = 0;
            wasDestroyed = false; // Indiquer que le mur est r�tabli
        }
    }

    // D�tecter les objets avec le tag "Growing" autour du mur
    private void DetectGrowingObjects()
    {
        Vector3 detectionCenter = transform.position; // Centre de d�tection correspondant � la position actuelle du mur
        Vector3 detectionSize = savedColliderSize; // Taille de la zone de d�tection

        Collider[] hitColliders = Physics.OverlapBox(detectionCenter, detectionSize / 2, Quaternion.identity); // R�cup�rer tous les colliders dans la zone de d�tection
        foreach (var hitCollider in hitColliders)
        {
            if (!string.IsNullOrEmpty(DestroyTag)&& hitCollider.CompareTag(DestroyTag) && !wasDestroyed)
            {
                DestroyWall(); // D�truire le mur si un objet avec le tag correct est d�tect�
                break; // Arr�ter la boucle apr�s avoir d�truit le mur
            }
        }
    }

    // V�rifier si des objets avec le tag "DestroyTag" sont pr�sents autour du mur
    private bool AreDestroyTagObjectsPresent()
    {
        Vector3 detectionCenter = transform.position; // Centre de d�tection correspondant � la position actuelle du mur
        Vector3 detectionSize = savedColliderSize; // Taille de la zone de d�tection

        Collider[] hitColliders = Physics.OverlapBox(detectionCenter, detectionSize / 2, Quaternion.identity); // R�cup�rer tous les colliders dans la zone de d�tection
        foreach (var hitCollider in hitColliders)
        {
            if (!string.IsNullOrEmpty(DestroyTag) && hitCollider.CompareTag(DestroyTag))
            {
                return true; // Renvoie vrai si des objets avec le tag correct sont pr�sents
            }
        }
        return false; // Renvoie faux si aucun objet avec le tag correct n'est pr�sent
    }

    // Visualiser la zone de d�tection avec des Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Couleur des Gizmos
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one); // Configurer la matrice de transformation des Gizmos
        Gizmos.DrawWireCube(Vector3.zero, savedColliderSize); // Dessiner un cube autour de la zone de d�tection
    }
}