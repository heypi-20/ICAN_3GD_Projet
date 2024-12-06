using UnityEngine;
using System.Collections.Generic;

public class S_SeedPlacer : MonoBehaviour
{
    [Header("Prefab Settings")]
    public List<GameObject> prefabList; // Liste des préfabriqués, définie via l'inspecteur
    public Transform previewPoint; // Point pour afficher le préfabriqué sélectionné
    public Transform spawnPoint; // Point de génération de l'objet
    public KeyCode placeKey = KeyCode.Space; // Touche pour générer l'objet, modifiable dans l'inspecteur

    private int currentIndex = 0; // Index du préfabriqué actuellement sélectionné
    private GameObject currentPreview; // Objet prévisualisé actuellement
    private List<GameObject> previewObjects = new List<GameObject>(); // Liste des objets de prévisualisation

    void Start()
    {
        // Pré-générer tous les objets préfabriqués pour les prévisualiser et les désactiver
        foreach (GameObject prefab in prefabList)
        {
            GameObject preview = Instantiate(prefab, previewPoint.position, Quaternion.identity, previewPoint);
            preview.transform.localScale *= 0.5f; // Réduire la taille à 30% de la taille normale pour la prévisualisation
            preview.SetActive(false);
            preview.GetComponent<Rigidbody>().useGravity = false;
            preview.GetComponent<BoxCollider>().enabled = false;
            previewObjects.Add(preview); // Ajouter à la liste des objets de prévisualisation
        }
    }

    void Update()
    {
        HandleScrollInput();
        HandlePlaceInput();
        FollowPreviewPoint();
    }

    void HandleScrollInput()
    {
        // Détection de la molette de la souris
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                currentIndex = (currentIndex + 1) % prefabList.Count;
            }
            else if (scroll < 0)
            {
                currentIndex = (currentIndex - 1 + prefabList.Count) % prefabList.Count;
            }
            UpdatePreview();
        }
    }

    void UpdatePreview()
    {
        // Si un objet est déjà en prévisualisation, le désactiver
        if (currentPreview != null)
        {
            currentPreview.SetActive(false);
        }

        // Activer l'objet préfabriqué actuellement sélectionné
        currentPreview = previewObjects[currentIndex];
        currentPreview.transform.position = previewPoint.position;
        currentPreview.SetActive(true);
    }

    void HandlePlaceInput()
    {
        // Générer l'objet préfabriqué actuel lorsque la touche spécifiée est enfoncée
        if (Input.GetKeyDown(placeKey) && currentPreview != null)
        {
            Instantiate(prefabList[currentIndex], spawnPoint.position, Quaternion.identity);
        }
    }

    void FollowPreviewPoint()
    {
        // Assurer que l'objet de prévisualisation suit toujours le previewPoint
        if (currentPreview != null)
        {
            currentPreview.transform.position = previewPoint.position;
        }
    }
}
