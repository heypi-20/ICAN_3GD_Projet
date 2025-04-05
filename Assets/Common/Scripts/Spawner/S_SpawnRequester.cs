using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SpawnRequester : MonoBehaviour
{
    public EnemyType EnemyType;
    public GameObject enemyprefab;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            Vector3[] spawnpoints = S_ZoneManager.Instance.GetSpawnPointsByEnemyWithZoneWeight(EnemyType);
            
            if (spawnpoints != null)
            {
                Debug.Log("Spawn points found"+spawnpoints.Length);    
                foreach (Vector3 pos in spawnpoints)
                {
                    Instantiate(enemyprefab, pos, Quaternion.identity);
                }
            }
        }
    }

}
