using System;
using System.Collections.Generic;
using UnityEngine;

public class _ZoneManager : MonoBehaviour
{
    private static _ZoneManager _instance;
    public static _ZoneManager Instance => _instance;

    public List<_SpawnZone> SpawnZones = new List<_SpawnZone>();
    private void Awake()
    {
        _instance = this;
    }
    
    private void Update()
    {
        //Todo change zone weights based on player pos and zone timer    
    }

    public Vector3[] GetSpawnPointsByEnemyType(_EnemyType type)
    {
        //todo get highest priority zone 
        foreach (_SpawnZone zone in SpawnZones)
        {
            if (zone.weight > 0f)
            {
                return zone.GetSpawnPointsByEnemyType(type);
            }
        }
        
        Debug.Log("No spawn points found    ");
        return null;
    }
}