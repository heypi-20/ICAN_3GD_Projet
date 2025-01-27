using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnDetails
    {
        public GameObject spawnType; // Type d'objet à générer
        public int spawnQuantity; // Quantité à générer par intervalle
        public float spawnRate; // Fréquence de génération (secondes entre chaque vague)
    }

    [System.Serializable]
    public class LevelSpawner
    {
        public int levelIndex; // Niveau requis (0 = tous les niveaux)
        public List<SpawnDetails> spawnDetails = new List<SpawnDetails>(); // Liste des configurations de génération pour ce niveau
    }

    [Header("Spawner Settings")]
    public List<LevelSpawner> levelSpawners = new List<LevelSpawner>(); // Liste des configurations par niveau
    private BoxCollider spawnArea; // Zone de génération basée sur un BoxCollider
    private S_EnergyStorage energyStorage; // Référence au stockage d'énergie
    [Header("Gizmos Settings")]
    public Color gizmoColor = new Color(0, 1, 0, 0.2f);

    private void Start()
    {
        spawnArea = gameObject.GetComponent<BoxCollider>();
        energyStorage = FindObjectOfType<S_EnergyStorage>();
        // Vérifier si un BoxCollider est assigné comme zone de génération
        if (spawnArea == null)
        {
            Debug.LogError("Aucun BoxCollider assigné comme zone de génération !");
            spawnArea = gameObject.AddComponent<BoxCollider>();
            spawnArea.isTrigger = true;
        }

        // Commencer la génération en continu
        StartCoroutine(SpawnObjectsCoroutine());
    }

    private IEnumerator SpawnObjectsCoroutine()
    {
        while (true)
        {
            if (energyStorage != null)
            {
                // Récupérer l'indice actuel du niveau énergétique (humanisé +1)
                int currentLevel = energyStorage.currentLevelIndex + 1;

                // Parcourir la liste des configurations
                foreach (var levelSpawner in levelSpawners)
                {
                    // Vérifier si le niveau est approprié pour cette configuration
                    if (levelSpawner.levelIndex == 0 || levelSpawner.levelIndex == currentLevel)
                    {
                        // Parcourir les détails de génération pour ce niveau
                        foreach (var detail in levelSpawner.spawnDetails)
                        {
                            // Générer les objets de manière échelonnée
                            yield return StartCoroutine(SpawnObjectsGradually(detail));

                            // Attendre la fréquence spécifiée avant la prochaine vague
                            yield return new WaitForSeconds(detail.spawnRate);
                        }
                    }
                }
            }

            yield return null; // Attendre une frame avant de recommencer
        }
    }

    private IEnumerator SpawnObjectsGradually(SpawnDetails detail)
    {
        for (int i = 0; i < detail.spawnQuantity; i++)
        {
            SpawnObject(detail.spawnType);
            yield return null; // Attendre une frame avant de générer le prochain objet
        }
    }

    private void SpawnObject(GameObject spawnType)
    {
        if (spawnType == null)
        {
            Debug.LogError("Aucun type d'objet spécifié pour la génération !");
            return;
        }

        // Calculer une position aléatoire dans la zone de génération basée sur le BoxCollider
        Vector3 randomPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
            Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
        );

        // Générer l'objet
        Instantiate(spawnType, randomPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        // Vérifier si un BoxCollider est assigné
        if (spawnArea == null)
        {
            spawnArea = GetComponent<BoxCollider>();
        }

        if (spawnArea != null)
        {
            // Définir la couleur du gizmo
            Gizmos.color = gizmoColor;

            // Dessiner un cube semi-transparent représentant la zone de génération
            Gizmos.matrix = spawnArea.transform.localToWorldMatrix;
            Gizmos.DrawCube(spawnArea.center, spawnArea.size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f); // Couleur opaque pour le contour
            Gizmos.DrawWireCube(spawnArea.center, spawnArea.size);
        }
    }
}
