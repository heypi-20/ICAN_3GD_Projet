using UnityEngine;

public class _EnemySpawnZone : MonoBehaviour
{
    public _EnemyType type;
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