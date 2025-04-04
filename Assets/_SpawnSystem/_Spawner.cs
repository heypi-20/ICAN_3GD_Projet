using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class _Spawner : MonoBehaviour
{
    [SerializeField] private _EnemyType type;
    [SerializeField] private GameObject enemyPrefab;
   [SerializeField] private AnimationCurve spawnRateCurve;
   
    [SerializeField] private AnimationCurve minQuantityByLevelCurve;
    [SerializeField] private AnimationCurve maxQuantityByLevelCurve;

    private float t;

    private int instanceCount;
    
    public void Update()
    {
        t += Time.deltaTime;
        
        if (t >= GetSpawnRateByCurrentEnemyCount(instanceCount))
        {
            t = 0f;
            Spawn();
        }
    }

    private void Spawn()
    {
        int qt = GetQuantityByCurrentEnemyCount(_GameState.Instance.PlayerLevel);
        //todo null check
        List<Vector3> points = _ZoneManager.Instance.GetSpawnPointsByEnemyType(type).ToList();
        for (int i = 0; i < qt; i++)
        {
            int rd = Random.Range(0, points.Count);
            Vector3 point = points[rd];
            points.RemoveAt(rd);
            
            //todo link to spawner ma,nager for pooling
            Instantiate(enemyPrefab, point, Quaternion.identity);
            instanceCount++;
        }
    }

    private float GetSpawnRateByCurrentEnemyCount(float level)
    {
        return spawnRateCurve.Evaluate(level);
    }

    private int GetQuantityByCurrentEnemyCount(float level)
    {
        return (int)minQuantityByLevelCurve.Evaluate(level);
    }
}