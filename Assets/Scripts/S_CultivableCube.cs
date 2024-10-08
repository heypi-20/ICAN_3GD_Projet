using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CultivableCube : MonoBehaviour
{
    public string growthTag = "Player";  // Tag qui déclenche la croissance
    public string growingStateTag = "Growing";  // Tag assigné lors de la croissance
    public bool isGrowing = false;  // Indique si le cube est en mode croissance
    public bool visualizeRange = true;  // Option de visualisation dans la scène
    public Color growingColor = Color.green;  // Couleur lorsque le cube est en mode croissance
    public Color defaultColor = Color.white;  // Couleur par défaut lorsque le cube n'est pas en mode croissance

    [Space (20)]
    [Header ("Gameplay Settings")]
    public GameObject growthPrefab;  // Préfabriqué à générer pendant la croissance
    public Vector3 growthRange = new Vector3(2f, 1f, 2f);  // Plage de croissance autour et au-dessus
    public int growthPerCycle = 1;  // Nombre de blocs générés par cycle
    public bool destroyAllOnHit = true;  // Détermine si tous les blocs sont détruits en une seule fois
    public int destroyAmountPerHit = 1;  // Nombre de blocs à détruire par coup si destroyAllOnHit est faux

    
    

    private Vector3 currentGrowthCenter; // La position actuelle du dernier point de croissance
    private Rigidbody rb;  // Référence au composant Rigidbody pour gérer la physique

    private float collisionCooldown = 1.0f; // Délai de refroidissement (en secondes)
    private bool isCooldown = false;  // Indique si le refroidissement est actif

    // S'abonner à l'événement de réinitialisation lors de l'activation
    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += Grow;
    }

    // Se désabonner de l'événement de réinitialisation lors de la désactivation
    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= Grow;
    }

    void Start()
    {
        isGrowing = false;
        // Obtenir le Rigidbody pour gérer le déplacement physique
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();  // Si le Rigidbody n'existe pas, en ajouter un
        }

        
        SetCubeColor(defaultColor);  // Mettre la couleur par défaut
    }

    // Détecter la collision avec un objet ayant le bon tag pour démarrer la croissance ou réinitialiser
    void OnCollisionEnter(Collision collision)
    {
        if (!isCooldown && collision.gameObject.CompareTag(growthTag))
        {
            if (isGrowing)
            {
                if (destroyAllOnHit)
                {
                    ResetGrowth();  // Tout détruire d'un coup
                }
                else
                {
                    DestroyGrowthBlocks(destroyAmountPerHit);  // Détruire un certain nombre de blocs
                }
            }
            else
            {
                // Démarrer la croissance si elle n'est pas active
                isGrowing = true;
                rb.isKinematic = true;  // Désactiver le mouvement physique lors de la croissance
                currentGrowthCenter = transform.position;
                SetTagsAndActiveCollider(growingStateTag);  // Ajouter le tag "Growing" à l'objet et ses enfants
                SetCubeColor(growingColor);  // Changer la couleur en mode croissance
                Debug.Log("Cube in growing mode.");
            }
            Destroy(collision.gameObject);
            // Activer le refroidissement pour empêcher les collisions rapides
            StartCoroutine(CollisionCooldownCoroutine());
        }
    }

    // Coroutine pour gérer le délai de refroidissement des collisions
    private IEnumerator CollisionCooldownCoroutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(collisionCooldown);
        isCooldown = false;
    }

    // Changer la couleur de tous les enfants uniquement
    private void SetCubeColor(Color color)
    {
        // Parcourir tous les enfants et changer leur couleur
        foreach (Transform child in transform)
        {
            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null)
            {
                childRenderer.material.color = color;
            }
        }
    }

    private void DestroyGrowthBlocks(int amount)
    {
        int blocksToDestroy = Mathf.Min(amount, transform.childCount - 1);  // S'assurer de ne pas détruire le centre

        for (int i = transform.childCount - 1; i > 0 && blocksToDestroy > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            blocksToDestroy--;
        }

        // Si un seul bloc reste, réinitialiser la croissance
        if (transform.childCount <= 1)
        {
            ResetGrowth();
        }
    }

    // Croissance du cube à chaque reset
    public void Grow()
    {
        if (!isGrowing) return;

        // Utiliser le dernier enfant comme centre de croissance
        if (transform.childCount > 0)
        {
            currentGrowthCenter = transform.GetChild(transform.childCount - 1).position;
        }

        // Générer plusieurs préfabriqués selon le nombre de "growthPerCycle"
        for (int i = 0; i < growthPerCycle; i++)
        {
            // Définir un point de croissance aléatoire dans la plage définie
            Vector3 randomPosition = currentGrowthCenter + new Vector3(
                Random.Range(-growthRange.x, growthRange.x),
                Random.Range(0, growthRange.y),  // Seulement vers le haut
                Random.Range(-growthRange.z, growthRange.z)
            );

            // Générer le préfabriqué à la nouvelle position
            GameObject newGrowth = Instantiate(growthPrefab, randomPosition, Quaternion.identity, this.transform);

            // Mettre à jour le centre de croissance à la nouvelle position
            currentGrowthCenter = newGrowth.transform.position;

            //Debug.Log("Nouveau préfabriqué généré à : " + randomPosition);
        }
        SetCubeColor(growingColor);
    }

    // Réinitialiser la croissance en détruisant tous les enfants sauf le centre
    private void ResetGrowth()
    {
        Debug.Log("Reset of the cube and deleting branches.");

        // Parcourir les enfants et détruire tous sauf le premier enfant
        for (int i = transform.childCount - 1; i > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Remettre le cube à l'état de départ
        isGrowing = false;
        rb.isKinematic = false;  // Autoriser de nouveau le mouvement physique
        RemoveTagsAndDeactiveCollider();  // Retirer le tag "Growing" de l'objet et de ses enfants
        SetCubeColor(defaultColor);  // Revenir à la couleur par défaut
    }

    // Activer le mode croissance
    private void SetTagsAndActiveCollider(string tag)
    {
        gameObject.tag = tag;
        foreach (Transform child in transform)
        {
            child.gameObject.tag = tag;
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null)
            {
                childCollider.enabled = true;  // Activer le collider lors de la croissance
            }
        }
    }

    // Désactiver le mode croissance et remettre à l'état de départ
    private void RemoveTagsAndDeactiveCollider()
    {
        gameObject.tag = "Cultivable";
        foreach (Transform child in transform)
        {
            child.gameObject.tag = "Cultivable";
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null)
            {
                childCollider.enabled = false;  // Désactiver le collider lors de la réinitialisation
            }
        }
    }

    // Visualiser la plage de croissance dans la scène avec des Gizmos
    private void OnDrawGizmos()
    {
        if (visualizeRange)
        {
            Vector3 gizmoCenter = currentGrowthCenter == Vector3.zero ? transform.position : currentGrowthCenter;
            gizmoCenter += new Vector3(0, growthRange.y / 2, 0);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(gizmoCenter, growthRange);
        }
    }
}
