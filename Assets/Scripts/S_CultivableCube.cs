using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CultivableCube : MonoBehaviour
{
    public string growthTag = "Player";  // Tag qui d�clenche la croissance
    public string growingStateTag = "Growing";  // Tag assign� lors de la croissance
    public bool isGrowing = false;  // Indique si le cube est en mode croissance
    public bool visualizeRange = true;  // Option de visualisation dans la sc�ne
    public Color growingColor = Color.green;  // Couleur lorsque le cube est en mode croissance
    public Color defaultColor = Color.white;  // Couleur par d�faut lorsque le cube n'est pas en mode croissance

    [Space (20)]
    [Header ("Param�tres de gameplay")]
    public GameObject growthPrefab;  // Pr�fabriqu� � g�n�rer pendant la croissance
    public Vector3 growthRange = new Vector3(2f, 1f, 2f);  // Plage de croissance autour et au-dessus
    public int growthPerCycle = 1;  // Nombre de blocs g�n�r�s par cycle
    public bool destroyAllOnHit = true;  // D�termine si tous les blocs sont d�truits en une seule fois
    public int destroyAmountPerHit = 1;  // Nombre de blocs � d�truire par coup si destroyAllOnHit est faux

    
    

    private Vector3 currentGrowthCenter; // La position actuelle du dernier point de croissance
    private Rigidbody rb;  // R�f�rence au composant Rigidbody pour g�rer la physique

    private float collisionCooldown = 1.0f; // D�lai de refroidissement (en secondes)
    private bool isCooldown = false;  // Indique si le refroidissement est actif

    // S'abonner � l'�v�nement de r�initialisation lors de l'activation
    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += Grow;
    }

    // Se d�sabonner de l'�v�nement de r�initialisation lors de la d�sactivation
    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= Grow;
    }

    void Start()
    {
        isGrowing = false;
        // Obtenir le Rigidbody pour g�rer le d�placement physique
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();  // Si le Rigidbody n'existe pas, en ajouter un
        }

        
        SetCubeColor(defaultColor);  // Mettre la couleur par d�faut
    }

    // D�tecter la collision avec un objet ayant le bon tag pour d�marrer la croissance ou r�initialiser
    void OnCollisionEnter(Collision collision)
    {
        if (!isCooldown && collision.gameObject.CompareTag(growthTag))
        {
            
            if (isGrowing)
            {
                if (destroyAllOnHit)
                {
                    ResetGrowth();  // Tout d�truire d'un coup
                }
                else
                {
                    DestroyGrowthBlocks(destroyAmountPerHit);  // D�truire un certain nombre de blocs
                }
            }
            else
            {
                // D�marrer la croissance si elle n'est pas active
                isGrowing = true;
                rb.isKinematic = true;  // D�sactiver le mouvement physique lors de la croissance
                currentGrowthCenter = transform.position;
                SetTags(growingStateTag);  // Ajouter le tag "Growing" � l'objet et ses enfants
                SetCubeColor(growingColor);  // Changer la couleur en mode croissance
                Debug.Log("Cube en mode croissance.");
            }
            Destroy(collision.gameObject);
            // Activer le refroidissement pour emp�cher les collisions rapides
            StartCoroutine(CollisionCooldownCoroutine());
        }
    }

    // Coroutine pour g�rer le d�lai de refroidissement des collisions
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
        int blocksToDestroy = Mathf.Min(amount, transform.childCount - 1);  // S'assurer de ne pas d�truire le centre

        for (int i = transform.childCount - 1; i > 0 && blocksToDestroy > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
            blocksToDestroy--;
        }

        // Si un seul bloc reste, r�initialiser la croissance
        if (transform.childCount <= 1)
        {
            ResetGrowth();
        }
    }

    // Croissance du cube � chaque reset
    public void Grow()
    {
        if (!isGrowing) return;

        // Utiliser le dernier enfant comme centre de croissance
        if (transform.childCount > 0)
        {
            currentGrowthCenter = transform.GetChild(transform.childCount - 1).position;
        }

        // G�n�rer plusieurs pr�fabriqu�s selon le nombre de "growthPerCycle"
        for (int i = 0; i < growthPerCycle; i++)
        {
            // D�finir un point de croissance al�atoire dans la plage d�finie
            Vector3 randomPosition = currentGrowthCenter + new Vector3(
                Random.Range(-growthRange.x, growthRange.x),
                Random.Range(0, growthRange.y),  // Seulement vers le haut
                Random.Range(-growthRange.z, growthRange.z)
            );

            // G�n�rer le pr�fabriqu� � la nouvelle position
            GameObject newGrowth = Instantiate(growthPrefab, randomPosition, Quaternion.identity, this.transform);

            // Mettre � jour le centre de croissance � la nouvelle position
            currentGrowthCenter = newGrowth.transform.position;

            //Debug.Log("Nouveau pr�fabriqu� g�n�r� � : " + randomPosition);
        }
        SetCubeColor(growingColor);
    }

    // R�initialiser la croissance en d�truisant tous les enfants sauf le centre
    private void ResetGrowth()
    {
        Debug.Log("R�initialisation du cube et suppression des branches.");

        // Parcourir les enfants et d�truire tous sauf le premier enfant
        for (int i = transform.childCount - 1; i > 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        // Remettre le cube � l'�tat de d�part
        isGrowing = false;
        rb.isKinematic = false;  // Autoriser de nouveau le mouvement physique
        RemoveTags();  // Retirer le tag "Growing" de l'objet et de ses enfants
        SetCubeColor(defaultColor);  // Revenir � la couleur par d�faut
    }

    // Ajouter un tag � l'objet et � tous ses enfants
    private void SetTags(string tag)
    {
        gameObject.tag = tag;
        foreach (Transform child in transform)
        {
            child.gameObject.tag = tag;
        }
    }

    // Retirer un tag en r�initialisant � "Untagged"
    private void RemoveTags()
    {
        gameObject.tag = "Untagged";
        foreach (Transform child in transform)
        {
            child.gameObject.tag = "Untagged";
        }
    }

    // Visualiser la plage de croissance dans la sc�ne avec des Gizmos
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
