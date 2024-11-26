using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class S_GrowthModeModule : MonoBehaviour
{
    public Vector3 growthDirection = Vector3.forward; // Direction de croissance (X, Y, Z)
    public float growthDistance = 1.0f; // Distance entre chaque objet de croissance
    public float randomGrowthFactor = 0.0f; // Facteur aléatoire pour la croissance (0 signifie pas de randomisation)
    public GameObject growthPrefab; // Préfab de l'objet de croissance
    public int initialPoolSize = 10; // Taille initiale de la pool d'objets
    [HideInInspector]
    public int maxPoolSize = 100; // Taille maximale de la pool d'objets
    public int maxGrowNumber;
    //public bool stopGrowingAtMaxPoolSize = true; // Arrêter de croître une fois la taille maximale atteinte
    public bool enableObstacleAvoidance = true; // Activer l'évitement des obstacles
    [HideInInspector]
    public int maxAttemptsToAvoidObstacle = 3; // Nombre maximum de tentatives pour éviter un obstacle

    private ObjectPool<Transform> growthPool;
    private List<Transform> branches = new List<Transform>(); // Liste des branches
    private Transform lastGrowthPoint; // Référence au dernier point de croissance
    private int currentAttemptCount = 0; // Compteur de tentatives actuelles
    private Vector3 lastTriedDirection = Vector3.zero; // Dernière direction essayée pour éviter les obstacles
    private bool canGrow = true; // Indique si la croissance est encore possible
    private int currentGrowCount;

    private void Start()
    {
        if (growthPrefab != null)
        {
            growthPool = new ObjectPool<Transform>(growthPrefab.transform, initialPoolSize, maxPoolSize);
            lastGrowthPoint = this.transform; // Initialiser le point de croissance à l'objet actuel
            branches.Add(this.transform); // Ajouter la position actuelle à la liste
        }
    }

    private void Update()
    {

        if (growthPool != null && growthPool.CurrentSize <= maxPoolSize * 0.05f)
        {
            StartCoroutine(RefillPool());
        }
        
        // Nettoyer les branches qui ont été détruites (si elles sont null)
        branches.RemoveAll(branch => branch == null);
        currentGrowCount = branches.Count-1;
    }

    public void Grow()
    {
        if (branches.Count <=1)
        {
            currentGrowCount = 0;
        }
        if (maxGrowNumber > -1)
        {
            if (CheckMaxGrowing())
            {
                return;
            }
        }
        
        if (growthPool != null && canGrow)
        {
            if (lastGrowthPoint == null)
            {
                lastGrowthPoint = branches.Count > 0 ? branches[branches.Count - 1] : this.transform;
            }

            Vector3 growthOffset = CalculateGrowthOffset();
            (Vector3 newGrowthPosition, Vector3 finalGrowthDirection) = enableObstacleAvoidance ? FindValidGrowthPosition(growthOffset) : (lastGrowthPoint.position + growthOffset, growthDirection);

            if (canGrow)
            {
                // Obtenir un nouvel objet de croissance depuis la pool
                Transform newGrowth = growthPool.GetObject();
                if (newGrowth != null)
                {
                    newGrowth.position = newGrowthPosition;
                    newGrowth.rotation = Quaternion.LookRotation(finalGrowthDirection); // Faire face à la direction de croissance
                    newGrowth.SetParent(this.transform); // Définir le nouvel objet comme enfant de l'objet actuel

                    // Mettre à jour le dernier point de croissance
                    lastGrowthPoint = newGrowth;
                    branches.Add(newGrowth); // Ajouter la nouvelle branche à la liste
                }
            }
        }
    }



    private bool CheckMaxGrowing()
    {
        if (currentGrowCount >= maxGrowNumber)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 CalculateGrowthOffset()
    {
        // Calculer la direction de croissance avec le facteur aléatoire
        float randomOffset = randomGrowthFactor != 0.0f ? UnityEngine.Random.Range(-randomGrowthFactor, randomGrowthFactor) : 0.0f;
        return growthDirection.normalized * (growthDistance + randomOffset);
    }

    private (Vector3, Vector3) FindValidGrowthPosition(Vector3 initialOffset)
    {
        int maxOverallAttempts = 10; // Limite pour éviter une boucle infinie
        int overallAttemptCount = 0;
        Vector3 currentDirection = initialOffset.normalized;
        Vector3 growthPosition = lastGrowthPoint.position + initialOffset;
        bool foundValidPosition = false;

        // Vérifier si la direction est bloquée par un obstacle
        while (Physics.Raycast(lastGrowthPoint.position, currentDirection, growthDistance) && overallAttemptCount < maxOverallAttempts)
        {
            currentAttemptCount++;
            overallAttemptCount++;

            if (currentAttemptCount >= maxAttemptsToAvoidObstacle)
            {
                // Réinitialiser le compteur de tentatives et essayer une direction différente de la dernière essayée
                currentAttemptCount = 0;
                Vector3[] alternativeDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
                Vector3 newDirection;
                do
                {
                    int randomIndex = UnityEngine.Random.Range(0, alternativeDirections.Length);
                    newDirection = alternativeDirections[randomIndex];
                } while (newDirection == lastTriedDirection);

                currentDirection = newDirection;
                lastTriedDirection = newDirection;
                growthPosition = lastGrowthPoint.position + currentDirection * growthDistance;
            }
            else
            {
                // Continuer à utiliser la direction actuelle jusqu'à atteindre le nombre maximal de tentatives
                growthPosition = lastGrowthPoint.position + currentDirection * growthDistance;
                continue;
            }

            // Vérifier si la direction originale est maintenant libre
            if (!Physics.Raycast(lastGrowthPoint.position, growthDirection, growthDistance))
            {
                growthPosition = lastGrowthPoint.position + growthDirection * growthDistance;
                currentDirection = growthDirection;
                foundValidPosition = true;
                break;
            }

            // Mettre à jour la position de croissance en fonction de la nouvelle direction
            growthPosition = lastGrowthPoint.position + currentDirection * growthDistance;
        }

        if (overallAttemptCount >= maxOverallAttempts && !foundValidPosition)
        {
            Vector3[] alternativeDirections = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            foreach (var dir in alternativeDirections)
            {
                if (!Physics.Raycast(lastGrowthPoint.position, dir, growthDistance))
                {
                    growthPosition = lastGrowthPoint.position + dir * growthDistance;
                    currentDirection = dir;
                    foundValidPosition = true;
                    break;
                }
            }

            if (!foundValidPosition)
            {
                Debug.LogWarning("Unable to find a valid growth position after multiple attempts. Growth halted.");
                StartCoroutine(CheckForGrowthSpace()); // Désactiver la croissance
            }
        }

        return (growthPosition, currentDirection);
    }

    private IEnumerator CheckForGrowthSpace()
    {
        while (!canGrow)
        {
            yield return new WaitForSeconds(1f);
            if (!Physics.Raycast(lastGrowthPoint.position, growthDirection, growthDistance))
            {
                Debug.Log("Space is now available. Resuming growth.");
                canGrow = true;
            }
        }
    }

    private IEnumerator RefillPool()
    {
        int refillAmount = 5; // Nombre d'objets à ajouter par batch
        while (growthPool.CurrentSize < maxPoolSize)
        {
            for (int i = 0; i < refillAmount && growthPool.CurrentSize < maxPoolSize; i++)
            {
                Transform newObject = GameObject.Instantiate(growthPrefab.transform);
                newObject.gameObject.SetActive(false);
                growthPool.ReturnObject(newObject);
            }
            yield return null; // Attendre une frame pour minimiser l'impact sur les performances
        }
    }
}

public class ObjectPool<T> where T : Component
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private int maxPoolSize;

    public ObjectPool(T prefab, int initialSize, int maxPoolSize)
    {
        this.prefab = prefab;
        this.maxPoolSize = maxPoolSize;

        for (int i = 0; i < initialSize; i++)
        {
            T newObject = GameObject.Instantiate(prefab);
            newObject.gameObject.SetActive(false);
            pool.Enqueue(newObject);
        }
    }

    public int CurrentSize => pool.Count;

    public T GetObject()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else if (pool.Count < maxPoolSize)
        {
            T newObject = GameObject.Instantiate(prefab);
            newObject.gameObject.SetActive(true);
            pool.Enqueue(newObject); // Ajouter le nouvel objet à la pool
            return newObject;
        }
        else
        {
            Debug.LogWarning("Pool has reached its maximum size.");
            return null;
        }
    }

    public void ReturnObject(T obj)
    {
        if (pool.Count < maxPoolSize)
        {
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
        else
        {
            GameObject.Destroy(obj.gameObject);
        }
    }
}
