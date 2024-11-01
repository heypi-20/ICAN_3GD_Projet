using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DestructibleWall : MonoBehaviour
{
    [Header("Prefab to spawn")]
    [Tooltip("L'effet de particule lors de la destruction du mur")]
    public GameObject destructionEffect;

    [Space(10)]
    [Tooltip("Le prefab à générer lors de la destruction (effet de débris ou encore plus)")]
    public GameObject shatterPrefab;

    [Tooltip("Nombre de préfabs à générer lors de la destruction")]
    public int numberOfShatterPieces = 5;

    [Tooltip("La force appliquée aux préfabs lors de la destruction")]
    public float shatterForce = 10f;

    [Space(10)]
    [Tooltip("Le prefab supplémentaire à générer lors de la destruction (Neutral Cube par exemple)")]
    public GameObject additionalPrefab;

    [Tooltip("Le nombre de réinitialisations avant de générer le prefab supplémentaire")]
    public int additionalPrefabSpawnRound = 1;

    [Space(20)]
    [Header("Destroy")]
    [Tooltip("Détruire l'objet qui touche le mur")] 
    public bool destroyTouchingObject = false;

    [Tooltip("Tag des objets qui doivent détruire le mur")] 
    public string DestroyTag = "Growing";

    [Tooltip("Nombre de réinitialisations avant la régénération du mur")] 
    public int regrowThreshold = 3;

    [Tooltip("Choix de détruire ou non les morceaux de débris lors de la réinitialisation")] 
    public bool destroyPiecesOnReset = true;

    private int currentResetRound = 0; // Compteur pour suivre le nombre de réinitialisations
    private bool wasDestroyed = false; // Indique si le mur a été détruit
    private Vector3 originalPosition; // Position initiale du mur
    private Quaternion originalRotation; // Rotation initiale du mur
    private Vector3 savedColliderSize; // Taille initiale du collider du mur
    private GameObject activeEffect; // Référence à l'effet de destruction actif
    private List<GameObject> shatterPieces = new List<GameObject>(); // Liste des morceaux de débris générés lors de la destruction
    private bool additionalPrefabGenerated = false; // Indique si le prefab supplémentaire a déjà été généré

    // S'abonner à l'événement de réinitialisation lors de l'activation de l'objet
    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += ResetDestroyedWall; // Ajouter la méthode ResetDestroyedWall à l'événement OnZoneReset
    }

    // Se désabonner de l'événement de réinitialisation lors de la désactivation de l'objet
    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= ResetDestroyedWall; // Retirer la méthode ResetDestroyedWall de l'événement OnZoneReset
    }

    void Start()
    {
        // Sauvegarder la position, la rotation initiales et la taille du collider du mur
        originalPosition = transform.position; // Sauvegarde de la position initiale
        originalRotation = transform.rotation; // Sauvegarde de la rotation initiale
        Collider collider = GetComponent<Collider>(); // Récupérer le collider du mur
        if (collider != null)
        {
            savedColliderSize = collider.bounds.size; // Sauvegarder la taille du collider si elle existe
        }
    }

    void FixedUpdate()
    {
        DetectGrowingObjects(); // Appeler la méthode pour détecter les objets en croissance autour du mur
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si l'objet en collision a le composant spécifique et si le mur n'est pas déjà détruit
        if (collision.gameObject.GetComponent<ThrownByThePlayer>() != null && !wasDestroyed)
        {
            DestroyWall(); // Détruire le mur
            // Choix de détruire l'objet qui touche le mur
            if (destroyTouchingObject)
            {
                Destroy(collision.gameObject); // Détruire l'objet en collision
            }
        }
    }

    // Logique pour détruire le mur
    private void DestroyWall()
    {
        wasDestroyed = true; // Indiquer que le mur est maintenant détruit
        additionalPrefabGenerated = false; // Réinitialiser le statut du prefab supplémentaire

        // Désactiver le MeshRenderer, Collider et Rigidbody du mur
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>(); // Récupérer le MeshRenderer
        Collider collider = GetComponent<Collider>(); // Récupérer le collider
        Rigidbody rigidbody = GetComponent<Rigidbody>(); // Récupérer le Rigidbody

        if (meshRenderer != null) meshRenderer.enabled = false; // Désactiver le MeshRenderer si présent
        if (collider != null) collider.enabled = false; // Désactiver le collider si présent
        if (rigidbody != null) rigidbody.detectCollisions = false; // Désactiver la détection des collisions si Rigidbody présent

        // Générer l'effet de destruction ou les morceaux de débris
        if (shatterPrefab != null)
        {
            GenerateShatterPieces(); // Appeler la méthode pour générer les morceaux de débris
        }
        else if (destructionEffect != null)
        {
            activeEffect = Instantiate(destructionEffect, transform.position, Quaternion.identity); // Générer l'effet de destruction
        }
    }

    // Générer les morceaux de débris
    private void GenerateShatterPieces()
    {
        for (int i = 0; i < numberOfShatterPieces; i++)
        {
            GameObject piece = Instantiate(shatterPrefab, transform.position, Random.rotation); // Générer un morceau de débris avec une rotation aléatoire
            Rigidbody pieceRb = piece.GetComponent<Rigidbody>(); // Récupérer le Rigidbody du morceau de débris
            if (pieceRb != null)
            {
                pieceRb.AddExplosionForce(shatterForce, transform.position, 5f); // Ajouter une force d'explosion aux morceaux de débris
            }
            shatterPieces.Add(piece); // Ajouter le morceau de débris à la liste des morceaux
        }
    }

    // Réinitialiser le mur
    public void ResetDestroyedWall()
    {
        // Ne réinitialiser que si le mur a été détruit
        if (GetComponent<MeshRenderer>().enabled)
        {
            Debug.Log("Le mur n'est pas détruit, pas besoin de réinitialiser."); // Afficher un message dans la console si le mur n'est pas détruit
            return; // Sortir de la méthode
        }
        currentResetRound++; // Incrémenter le compteur de réinitialisations

        // Générer un prefab supplémentaire après un certain nombre de réinitialisations
        if (wasDestroyed && !additionalPrefabGenerated && currentResetRound >= additionalPrefabSpawnRound && additionalPrefab != null)
        {
            Instantiate(additionalPrefab, transform.position, Quaternion.identity); // Générer le prefab supplémentaire
            additionalPrefabGenerated = true; // Indiquer que le prefab supplémentaire a été généré
        }

        // Vérifier si des objets avec le tag "DestroyTag" sont présents
        if (AreDestroyTagObjectsPresent())
        {
            Debug.Log("Des objets avec le tag 'DestroyTag' sont toujours présents, réinitialisation du mur reportée."); // Afficher un message dans la console
            return; // Sortir de la méthode
        }

        // Si le mur doit être réactivé après un certain nombre de réinitialisations
        if (wasDestroyed && currentResetRound >= regrowThreshold)
        {
            // Réactiver le MeshRenderer, Collider, et Rigidbody du mur
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Collider collider = GetComponent<Collider>();
            Rigidbody rigidbody = GetComponent<Rigidbody>();

            if (meshRenderer != null) meshRenderer.enabled = true; // Réactiver le MeshRenderer
            if (collider != null) collider.enabled = true; // Réactiver le collider
            if (rigidbody != null) rigidbody.detectCollisions = true; // Réactiver la détection des collisions du Rigidbody

            // Réinitialiser la position et la rotation du mur
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            // Détruire l'effet de destruction
            if (activeEffect != null)
            {
                Destroy(activeEffect); // Détruire l'effet de particule
            }

            // Détruire les morceaux de débris si nécessaire
            if (destroyPiecesOnReset)
            {
                foreach (GameObject piece in shatterPieces)
                {
                    Destroy(piece); // Détruire chaque morceau de débris
                }
                shatterPieces.Clear(); // Vider la liste des morceaux de débris
            }

            // Réinitialiser le compteur de réinitialisations
            currentResetRound = 0;
            wasDestroyed = false; // Indiquer que le mur est rétabli
        }
    }

    // Détecter les objets avec le tag "Growing" autour du mur
    private void DetectGrowingObjects()
    {
        Vector3 detectionCenter = transform.position; // Centre de détection correspondant à la position actuelle du mur
        Vector3 detectionSize = savedColliderSize; // Taille de la zone de détection

        Collider[] hitColliders = Physics.OverlapBox(detectionCenter, detectionSize / 2, Quaternion.identity); // Récupérer tous les colliders dans la zone de détection
        foreach (var hitCollider in hitColliders)
        {
            if (!string.IsNullOrEmpty(DestroyTag)&& hitCollider.CompareTag(DestroyTag) && !wasDestroyed)
            {
                DestroyWall(); // Détruire le mur si un objet avec le tag correct est détecté
                break; // Arrêter la boucle après avoir détruit le mur
            }
        }
    }

    // Vérifier si des objets avec le tag "DestroyTag" sont présents autour du mur
    private bool AreDestroyTagObjectsPresent()
    {
        Vector3 detectionCenter = transform.position; // Centre de détection correspondant à la position actuelle du mur
        Vector3 detectionSize = savedColliderSize; // Taille de la zone de détection

        Collider[] hitColliders = Physics.OverlapBox(detectionCenter, detectionSize / 2, Quaternion.identity); // Récupérer tous les colliders dans la zone de détection
        foreach (var hitCollider in hitColliders)
        {
            if (!string.IsNullOrEmpty(DestroyTag) && hitCollider.CompareTag(DestroyTag))
            {
                return true; // Renvoie vrai si des objets avec le tag correct sont présents
            }
        }
        return false; // Renvoie faux si aucun objet avec le tag correct n'est présent
    }

    // Visualiser la zone de détection avec des Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Couleur des Gizmos
        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one); // Configurer la matrice de transformation des Gizmos
        Gizmos.DrawWireCube(Vector3.zero, savedColliderSize); // Dessiner un cube autour de la zone de détection
    }
}