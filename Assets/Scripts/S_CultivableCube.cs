using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CultivableCube : MonoBehaviour
{
    [Tooltip("Tag attribué au cube lorsque la croissance est active")]
    public string growingTag = "Growing";

    [Tooltip("Prefab à générer lors de la croissance")]
    public GameObject growthPrefab;

    [Tooltip("Plage de croissance autour et au-dessus du cube")]
    public Vector3 growthRangeUp = new Vector3(2f, 1f, 2f);
    public Vector3 growthRangeDown = new Vector3(2f, 1f, 2f);
    [Tooltip("Nombre de blocs générés par cycle de croissance")]
    public int growthPerCycle = 1;

    [Tooltip("Détermine si tous les blocs sont détruits en une seule fois lors d'un coup")]
    public bool destroyAllOnHit = true;

    [Tooltip("Nombre de blocs à détruire par coup si destroyAllOnHit est faux")]
    public int destroyAmountPerHit = 1;

    public bool isGrowing = false; // Indique si le cube est en train de croître

    public Color ColorDefault; // Couleur par défaut du cube
    public Color ColorGrowUp; // Couleur lorsque le cube est en croissance
    private Vector3 currentGrowthCenter; // La position actuelle du dernier point de croissance
    private Rigidbody rb; // Référence au composant Rigidbody pour gérer la physique
    private List<GameObject> growthInstances = new List<GameObject>(); // Liste des préfabs générés lors de la croissance
    private float collisionCooldown = 1.0f; // Délai de refroidissement (en secondes)
    private bool isCooldown = false; // Indique si le refroidissement est actif

    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += Grow; // S'abonner à l'événement de reset de zone
    }

    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= Grow; // Se désabonner de l'événement de reset de zone
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtenir le Rigidbody pour gérer le déplacement physique
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>(); // Si le Rigidbody n'existe pas, en ajouter un
        }
        SetCubeColor(ColorDefault); // Mettre la couleur par défaut

        if (isGrowing)
        {
            StartGrowth(); // Démarrer la croissance si déjà activée
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isCooldown && collision.gameObject.GetComponent<ThrownByThePlayer>() != null) // Vérifier si l'objet en collision a le bon composant
        {
            if (isGrowing)
            {
                if (destroyAllOnHit)
                {
                    ResetGrowth(); // Tout détruire d'un coup
                }
                else
                {
                    DestroyGrowthBlocks(destroyAmountPerHit); // Détruire un certain nombre de blocs
                }
            }
            else
            {
                StartGrowth(); // Démarrer la croissance si elle n'est pas active
            }
            Destroy(collision.gameObject); // Détruire l'objet en collision
            StartCoroutine(CollisionCooldownCoroutine()); // Activer le refroidissement pour empêcher les collisions rapides
        }
    }

    private void StartGrowth()
    {
        EnableChildColliders(); // Activer les colliders des enfants
        AssignTagToChildren(growingTag); // Assigner le tag aux enfants
        StartCoroutine(WaitUntilStoppedAndGrow()); // Attendre l'arrêt du cube avant de croître
    }

    private void EnableChildColliders()
    {
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            Collider childCollider = child.GetComponent<Collider>(); // Obtenir le collider de l'enfant
            if (childCollider != null)
            {
                childCollider.enabled = true; // Activer le collider de l'enfant
            }
        }
    }

    private void DisableChildColliders()
    {
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            Collider childCollider = child.GetComponent<Collider>(); // Obtenir le collider de l'enfant
            if (childCollider != null)
            {
                childCollider.enabled = false; // Désactiver le collider de l'enfant
            }
        }
    }
    private bool isGrowingProcessFinished = false;

    private IEnumerator WaitUntilStoppedAndGrow()
    {
        yield return new WaitForSeconds(0.1f); // Attendre un court délai
        yield return new WaitUntil(() => rb.velocity.sqrMagnitude < 0.01f); // Attendre que la vitesse du Rigidbody soit presque nulle
        rb.isKinematic = true; // Désactiver le mouvement physique lors de la croissance
        isGrowing = true; // Définir le cube comme étant en croissance
        gameObject.tag = growingTag; // Assigner le tag de croissance
        currentGrowthCenter = transform.position; // Définir le centre de croissance actuel
        SetCubeColor(ColorGrowUp); // Changer la couleur en mode croissance
        isGrowingProcessFinished=true;
        Debug.Log("Done");
    }

    public void Grow()
    {
        if (!(isGrowing && isGrowingProcessFinished))
        {
            Debug.Log("NotReady");
            return;
        } 
        

        for (int i = 0; i < growthPerCycle; i++) // Générer le nombre de blocs par cycle
        {
            Vector3 randomPosition = currentGrowthCenter + new Vector3(
                Random.Range(growthRangeDown.x, growthRangeUp.x), // Calculer une position aléatoire dans la plage de croissance
                Random.Range(growthRangeDown.y, growthRangeUp.y), // Seulement vers le haut
                Random.Range(growthRangeDown.z, growthRangeUp.z)
            );

            GameObject newGrowth = Instantiate(growthPrefab, randomPosition, Quaternion.identity, this.transform); // Instancier le prefab de croissance
            growthInstances.Add(newGrowth); // Ajouter le nouvel objet à la liste
            currentGrowthCenter = newGrowth.transform.position; // Mettre à jour le centre de croissance
        }
    }

    private void ResetGrowth()
    {
        DisableChildColliders(); // Désactiver les colliders des enfants
        RemoveTagFromChildren(); // Retirer le tag de croissance des enfants
        Debug.Log("Reset of the cube and deleting branches."); // Log pour déboguer
        isGrowing = false; // Définir le cube comme n'étant plus en croissance
        rb.isKinematic = false; // Réactiver le mouvement physique
        gameObject.tag = "Untagged"; // Retirer le tag de croissance du cube
        SetCubeColor(ColorDefault); // Revenir à la couleur par défaut

        foreach (GameObject growth in growthInstances) // Détruire tous les blocs générés
        {
            Destroy(growth);
        }
        growthInstances.Clear(); // Vider la liste des blocs générés
    }

    private void DestroyGrowthBlocks(int amount)
    {
        int blocksToDestroy = Mathf.Min(amount, growthInstances.Count); // Déterminer le nombre de blocs à détruire
        for (int i = 0; i < blocksToDestroy; i++)
        {
            int lastIndex = growthInstances.Count - 1; // Obtenir l'index du dernier bloc
            Destroy(growthInstances[lastIndex]); // Détruire le dernier bloc
            growthInstances.RemoveAt(lastIndex); // Retirer le bloc de la liste
        }

        if (growthInstances.Count == 0) // Si tous les blocs sont détruits, réinitialiser la croissance
        {
            ResetGrowth();
        }
    }

    private IEnumerator CollisionCooldownCoroutine()
    {
        isCooldown = true; // Activer le cooldown des collisions
        yield return new WaitForSeconds(collisionCooldown); // Attendre la fin du délai de refroidissement
        isCooldown = false; // Désactiver le cooldown
    }

    private void SetCubeColor(Color color)
    {
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            Renderer childRenderer = child.GetComponent<Renderer>(); // Obtenir le renderer de l'enfant
            if (childRenderer != null)
            {
                childRenderer.material.color = color; // Changer la couleur du matériel
            }
        }
    }

    private void AssignTagToChildren(string tag)
    {
        gameObject.tag = tag; // Assigner le tag au cube
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            child.tag = tag; // Assigner le tag à chaque enfant
        }
    }

    private void RemoveTagFromChildren()
    {
        gameObject.tag = "Untagged"; // Retirer le tag du cube
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            child.tag = "Untagged"; // Retirer le tag de chaque enfant
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Définir la couleur des Gizmos
        Vector3 gizmoCenter = currentGrowthCenter == Vector3.zero ? transform.position : currentGrowthCenter; // Déterminer le centre des Gizmos
        Gizmos.DrawWireCube(gizmoCenter, growthRangeUp);
        // Dessiner un cube filaire représentant la plage de croissance
    }
}