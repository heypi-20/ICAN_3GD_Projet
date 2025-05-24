using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGeyser : MonoBehaviour
{
    [Header("Objet à instancier")]
    public GameObject prefabToSpawn;

    [Header("Durée de vie du prefab")]
    public float lifeTime = 5f;

    [Header("Intervalle de spawn (en secondes)")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

    private BoxCollider spawnZone;

    void Start()
    {
        spawnZone = GetComponent<BoxCollider>();

        if (!spawnZone || !spawnZone.isTrigger)
        {
            Debug.LogError("Le SpawnerZone nécessite un BoxCollider en mode Trigger !");
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            Spawn();
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void Spawn()
    {
        if (!prefabToSpawn || spawnZone == null) return;

        Vector3 center = spawnZone.bounds.center;
        Vector3 size = spawnZone.bounds.size;

        float randomX = Random.Range(center.x - size.x / 2f, center.x + size.x / 2f);
        float randomZ = Random.Range(center.z - size.z / 2f, center.z + size.z / 2f);
        Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ);

        GameObject obj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        Destroy(obj, lifeTime);
    }
}
