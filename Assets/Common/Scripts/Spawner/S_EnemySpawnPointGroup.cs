using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Holds spawn point positions for a specific enemy type.
/// </summary>
public class S_EnemySpawnPointGroup : MonoBehaviour
{
    // The type of enemy this group is for.
    public EnemyType enemyType;
    
    // An array that will hold the positions of spawn points.
    public Vector3[] spawnPoints;

    // Awake is called when the script instance is being loaded.
    private void Awake()
    {
        // Create an array with a length equal to the number of child objects.
        spawnPoints = new Vector3[transform.childCount];
        int i = 0;
        
        // Loop through each child of this GameObject.
        foreach (Transform child in transform)
        {
            // Store the child's position in the array.
            spawnPoints[i] = child.position;
            i++;
        }
    }
}