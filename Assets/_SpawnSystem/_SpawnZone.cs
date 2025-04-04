using System.Collections.Generic;
using UnityEngine;

public class _SpawnZone : MonoBehaviour
{
    public  List<_EnemySpawnZone> _enemySpawnZones;
    public float weight;
    
    //zone DURATION
    
    public Vector3[] GetSpawnPointsByEnemyType(_EnemyType type)
    {
        foreach (_EnemySpawnZone zone in _enemySpawnZones)
        {
            if (zone.type == type)
            {
                return zone.spawnPoints;
            }
        }

        Debug.Log("No spawn points found    ");
        return null;
    }
}