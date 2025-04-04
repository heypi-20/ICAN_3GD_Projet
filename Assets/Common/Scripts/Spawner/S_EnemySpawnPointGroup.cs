using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class S_EnemySpawnPointGroup : MonoBehaviour
{
    public EnemyType enemyType;
    public Vector3[] spawnPoints;
    private void Awake()
    {
        spawnPoints = new Vector3[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            spawnPoints[i] = child.position;
            i++;
        }
    }

}
