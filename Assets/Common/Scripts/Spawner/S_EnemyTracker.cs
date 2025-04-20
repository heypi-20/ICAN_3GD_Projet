using System.Collections.Generic;
using UnityEngine;

// Tracks the number of active enemies for each enemy type.
public class S_EnemyTracker
{
    // A dictionary that stores a set of enemy GameObjects for each enemy type.
    private Dictionary<EnemyType, HashSet<GameObject>> enemyInstances = new();

    // Registers an enemy type by creating a new set if it doesn't exist.
    public void RegisterType(EnemyType type)
    {
        if (!enemyInstances.ContainsKey(type))
            enemyInstances[type] = new HashSet<GameObject>();
    }

    // Adds an enemy GameObject to the set for its type.
    public void Add(EnemyType type, GameObject enemy)
    {
        if (!enemyInstances.ContainsKey(type))
            enemyInstances[type] = new HashSet<GameObject>();

        enemyInstances[type].Add(enemy);
    }

    // Removes an enemy GameObject from the sets.
    public void Remove(EnemyType type, GameObject enemy)
    {
        if (enemyInstances.TryGetValue(type, out var set))
        {
            set.Remove(enemy);
        }
    }

    // Returns the number of active enemy GameObjects for a specific enemy type.
    public int GetCount(EnemyType type)
    {
        if (!enemyInstances.ContainsKey(type))
            return 0;
        // Remove any null entries (destroyed objects) from the set.
        enemyInstances[type].RemoveWhere(e => e == null);
        return enemyInstances[type].Count;
    }

    // Clears all tracked enemy data.
    public void Clear()
    {
        enemyInstances.Clear();
    }
}