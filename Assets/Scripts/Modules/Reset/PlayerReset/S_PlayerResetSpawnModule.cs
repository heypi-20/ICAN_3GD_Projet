using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerResetSpawnModule : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableObject
    {
        [System.Serializable]
        public class ObjectToSpawn
        {
            public GameObject obj; // L'objet � g�n�rer
            public int spawnCount = 1; // Nombre d'instances de cet objet � g�n�rer
        }

        public List<ObjectToSpawn> objs = new List<ObjectToSpawn>(); // Liste des objets � g�n�rer avec leurs quantit�s
        public Transform specifiedPosition; // Position sp�cifi�e pour la g�n�ration
        public GameObject spawnArea; // Zone de g�n�ration (box de taille d'un autre objet)
        public bool centralizeSpawn = true; // G�n�rer les objets de mani�re centralis�e dans la zone
        public bool destroyPreviousSpawn = false; // D�truire les objets g�n�r�s pr�c�demment ?
        public int spawnInterval = 1; // Nombre d'appels � SpawnObjects() avant de g�n�rer les objets

        [HideInInspector]
        public int currentSpawnCallCount = 0; // Compteur d'appels de la m�thode SpawnObjects()
    }

    public List<SpawnableObject> spawnableObjectsList = new List<SpawnableObject>(); // Liste des objets g�n�rables
    public string exemptComponentName = ""; // Nom du composant exempt de destruction (vide signifie aucun composant exempt)

    private List<GameObject> previousSpawnedObjects = new List<GameObject>();



    public void PlayerSpawnObjects()
    {
        foreach (SpawnableObject spawnable in spawnableObjectsList)
        {
            spawnable.currentSpawnCallCount++;

            if (ShouldSpawn(spawnable))
            {
                spawnable.currentSpawnCallCount = 0;

                if (spawnable.destroyPreviousSpawn)
                {
                    DestroyPreviousSpawnedObjects();
                }

                StartCoroutine(SpawnNewObjectsCoroutine(spawnable));
            }
        }
    }

    private bool ShouldSpawn(SpawnableObject spawnable)
    {
        return spawnable.currentSpawnCallCount >= spawnable.spawnInterval;
    }

    private IEnumerator SpawnNewObjectsCoroutine(SpawnableObject spawnable)
    {
        foreach (SpawnableObject.ObjectToSpawn objectToSpawn in spawnable.objs)
        {
            for (int i = 0; i < objectToSpawn.spawnCount; i++)
            {
                Vector3 spawnPosition = GetSpawnPosition(spawnable);
                if (spawnPosition != Vector3.zero && objectToSpawn.obj != null)
                {
                    GameObject spawnedObj = Instantiate(objectToSpawn.obj, spawnPosition, Quaternion.identity);
                    previousSpawnedObjects.Add(spawnedObj);
                }
                yield return null; // Attendre une frame pour r�partir la charge sur plusieurs frames
            }
        }
    }

    private Vector3 GetSpawnPosition(SpawnableObject spawnable)
    {
        if (spawnable.specifiedPosition != null)
        {
            return spawnable.specifiedPosition.position;
        }
        else if (spawnable.spawnArea != null)
        {
            BoxCollider boxCollider = spawnable.spawnArea.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                return GetPositionWithinBox(boxCollider, spawnable.centralizeSpawn);
            }
            else
            {
                Debug.LogWarning("Spawn area does not have a BoxCollider component.");
            }
        }
        else
        {
            Debug.LogWarning("Neither specified position nor spawn area is defined for spawning.");
        }
        return Vector3.zero;
    }

    private void DestroyPreviousSpawnedObjects()
    {
        for (int i = previousSpawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = previousSpawnedObjects[i];
            if (obj != null)
            {
                if (ShouldDestroyObject(obj))
                {
                    Destroy(obj);
                    previousSpawnedObjects.RemoveAt(i);
                }
                else
                {
                    previousSpawnedObjects.RemoveAt(i);
                }
            }
        }
    }

    private bool ShouldDestroyObject(GameObject obj)
    {
        return string.IsNullOrEmpty(exemptComponentName) || obj.GetComponent(exemptComponentName) == null;
    }

    private Vector3 GetPositionWithinBox(BoxCollider boxCollider, bool centralize)
    {
        Vector3 center = boxCollider.transform.position;
        Vector3 size = boxCollider.size * 0.5f;
        Vector3 randomOffset = new Vector3(
            Random.Range(-size.x, size.x),
            Random.Range(-size.y, size.y),
            Random.Range(-size.z, size.z)
        );

        return centralize ? center + randomOffset * 0.3f : center + randomOffset;
    }
}
