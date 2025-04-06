using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 追踪场上每种类型敌人数量
/// </summary>
public class S_EnemyTracker
{
    private Dictionary<EnemyType, HashSet<GameObject>> enemyInstances = new();

    public void RegisterType(EnemyType type)
    {
        if (!enemyInstances.ContainsKey(type))
            enemyInstances[type] = new HashSet<GameObject>();
    }

    public void Add(EnemyType type, GameObject enemy)
    {
        if (!enemyInstances.ContainsKey(type))
            enemyInstances[type] = new HashSet<GameObject>();

        enemyInstances[type].Add(enemy);
    }

    public void Remove(GameObject enemy)
    {
        foreach (var list in enemyInstances.Values)
        {
            if (list.Remove(enemy))
                break;
        }
    }

    public int GetCount(EnemyType type)
    {
        if (!enemyInstances.ContainsKey(type)) return 0;
        enemyInstances[type].RemoveWhere(e => e == null);
        return enemyInstances[type].Count;
    }

    public void Clear()
    {
        enemyInstances.Clear();
    }
}