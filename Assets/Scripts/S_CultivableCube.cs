using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CultivableCube : MonoBehaviour
{
    [Tooltip("Tag attribu� au cube lorsque la croissance est active")]
    public string growingTag = "Growing";

    [Tooltip("Prefab � g�n�rer lors de la croissance")]
    public GameObject growthPrefab;

    [Tooltip("Plage de croissance autour et au-dessus du cube")]
    public Vector3 growthRangeUp = new Vector3(2f, 1f, 2f);
    public Vector3 growthRangeDown = new Vector3(2f, 1f, 2f);
    [Tooltip("Nombre de blocs g�n�r�s par cycle de croissance")]
    public int growthPerCycle = 1;

    [Tooltip("D�termine si tous les blocs sont d�truits en une seule fois lors d'un coup")]
    public bool destroyAllOnHit = true;

    [Tooltip("Nombre de blocs � d�truire par coup si destroyAllOnHit est faux")]
    public int destroyAmountPerHit = 1;

    public bool isGrowing = false; // Indique si le cube est en train de cro�tre

    public Color ColorDefault; // Couleur par d�faut du cube
    public Color ColorGrowUp; // Couleur lorsque le cube est en croissance
    private Vector3 currentGrowthCenter; // La position actuelle du dernier point de croissance
    private Rigidbody rb; // R�f�rence au composant Rigidbody pour g�rer la physique
    private List<GameObject> growthInstances = new List<GameObject>(); // Liste des pr�fabs g�n�r�s lors de la croissance
    private float collisionCooldown = 1.0f; // D�lai de refroidissement (en secondes)
    private bool isCooldown = false; // Indique si le refroidissement est actif

    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += Grow; // S'abonner � l'�v�nement de reset de zone
    }

    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= Grow; // Se d�sabonner de l'�v�nement de reset de zone
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Obtenir le Rigidbody pour g�rer le d�placement physique
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>(); // Si le Rigidbody n'existe pas, en ajouter un
        }
        SetCubeColor(ColorDefault); // Mettre la couleur par d�faut

        if (isGrowing)
        {
            StartGrowth(); // D�marrer la croissance si d�j� activ�e
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isCooldown && collision.gameObject.GetComponent<ThrownByThePlayer>() != null) // V�rifier si l'objet en collision a le bon composant
        {
            if (isGrowing)
            {
                if (destroyAllOnHit)
                {
                    ResetGrowth(); // Tout d�truire d'un coup
                }
                else
                {
                    DestroyGrowthBlocks(destroyAmountPerHit); // D�truire un certain nombre de blocs
                }
            }
            else
            {
                StartGrowth(); // D�marrer la croissance si elle n'est pas active
            }
            Destroy(collision.gameObject); // D�truire l'objet en collision
            StartCoroutine(CollisionCooldownCoroutine()); // Activer le refroidissement pour emp�cher les collisions rapides
        }
    }

    private void StartGrowth()
    {
        EnableChildColliders(); // Activer les colliders des enfants
        AssignTagToChildren(growingTag); // Assigner le tag aux enfants
        StartCoroutine(WaitUntilStoppedAndGrow()); // Attendre l'arr�t du cube avant de cro�tre
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
                childCollider.enabled = false; // D�sactiver le collider de l'enfant
            }
        }
    }
    private bool isGrowingProcessFinished = false;

    private IEnumerator WaitUntilStoppedAndGrow()
    {
        yield return new WaitForSeconds(0.1f); // Attendre un court d�lai
        yield return new WaitUntil(() => rb.velocity.sqrMagnitude < 0.01f); // Attendre que la vitesse du Rigidbody soit presque nulle
        rb.isKinematic = true; // D�sactiver le mouvement physique lors de la croissance
        isGrowing = true; // D�finir le cube comme �tant en croissance
        gameObject.tag = growingTag; // Assigner le tag de croissance
        currentGrowthCenter = transform.position; // D�finir le centre de croissance actuel
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
        

        for (int i = 0; i < growthPerCycle; i++) // G�n�rer le nombre de blocs par cycle
        {
            Vector3 randomPosition = currentGrowthCenter + new Vector3(
                Random.Range(growthRangeDown.x, growthRangeUp.x), // Calculer une position al�atoire dans la plage de croissance
                Random.Range(growthRangeDown.y, growthRangeUp.y), // Seulement vers le haut
                Random.Range(growthRangeDown.z, growthRangeUp.z)
            );

            GameObject newGrowth = Instantiate(growthPrefab, randomPosition, Quaternion.identity, this.transform); // Instancier le prefab de croissance
            growthInstances.Add(newGrowth); // Ajouter le nouvel objet � la liste
            currentGrowthCenter = newGrowth.transform.position; // Mettre � jour le centre de croissance
        }
    }

    private void ResetGrowth()
    {
        DisableChildColliders(); // D�sactiver les colliders des enfants
        RemoveTagFromChildren(); // Retirer le tag de croissance des enfants
        Debug.Log("Reset of the cube and deleting branches."); // Log pour d�boguer
        isGrowing = false; // D�finir le cube comme n'�tant plus en croissance
        rb.isKinematic = false; // R�activer le mouvement physique
        gameObject.tag = "Untagged"; // Retirer le tag de croissance du cube
        SetCubeColor(ColorDefault); // Revenir � la couleur par d�faut

        foreach (GameObject growth in growthInstances) // D�truire tous les blocs g�n�r�s
        {
            Destroy(growth);
        }
        growthInstances.Clear(); // Vider la liste des blocs g�n�r�s
    }

    private void DestroyGrowthBlocks(int amount)
    {
        int blocksToDestroy = Mathf.Min(amount, growthInstances.Count); // D�terminer le nombre de blocs � d�truire
        for (int i = 0; i < blocksToDestroy; i++)
        {
            int lastIndex = growthInstances.Count - 1; // Obtenir l'index du dernier bloc
            Destroy(growthInstances[lastIndex]); // D�truire le dernier bloc
            growthInstances.RemoveAt(lastIndex); // Retirer le bloc de la liste
        }

        if (growthInstances.Count == 0) // Si tous les blocs sont d�truits, r�initialiser la croissance
        {
            ResetGrowth();
        }
    }

    private IEnumerator CollisionCooldownCoroutine()
    {
        isCooldown = true; // Activer le cooldown des collisions
        yield return new WaitForSeconds(collisionCooldown); // Attendre la fin du d�lai de refroidissement
        isCooldown = false; // D�sactiver le cooldown
    }

    private void SetCubeColor(Color color)
    {
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            Renderer childRenderer = child.GetComponent<Renderer>(); // Obtenir le renderer de l'enfant
            if (childRenderer != null)
            {
                childRenderer.material.color = color; // Changer la couleur du mat�riel
            }
        }
    }

    private void AssignTagToChildren(string tag)
    {
        gameObject.tag = tag; // Assigner le tag au cube
        foreach (Transform child in transform) // Parcourir tous les enfants
        {
            child.tag = tag; // Assigner le tag � chaque enfant
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
        Gizmos.color = Color.green; // D�finir la couleur des Gizmos
        Vector3 gizmoCenter = currentGrowthCenter == Vector3.zero ? transform.position : currentGrowthCenter; // D�terminer le centre des Gizmos
        Gizmos.DrawWireCube(gizmoCenter, growthRangeUp);
        // Dessiner un cube filaire repr�sentant la plage de croissance
    }
}