using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DestroySelfAfterCollision : MonoBehaviour
{
    // The maximum collision count before self-destruction, adjustable in Inspector
    [SerializeField]
    private int maxCollisionCount = 3;

    // Counter to keep track of collisions with S_DestructibleWall
    private int collisionCount = 0;

    // Layer mask to specify which layers should be considered for collisions
    [SerializeField]
    private LayerMask destructibleLayerMask;

    // Define the length of the ray or size of the box
    [SerializeField]
    private float detectionDistance = 1.0f;

    private Vector3 boxSize;
    private HashSet<Collider> processedColliders = new HashSet<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        // Set boxSize to match the object's scale
        boxSize = transform.localScale;
    }

    // Update method to perform raycast or boxcast to detect collisions
    void Update()
    {
        // Perform a boxcast to detect objects in front of this GameObject
        RaycastHit[] hits = Physics.BoxCastAll(transform.position, boxSize / 2, transform.forward, Quaternion.identity, detectionDistance, destructibleLayerMask);

        foreach (RaycastHit hit in hits)
        {
            // Check if the hit object has the S_DestructibleWall component and hasn't been processed already
            if (hit.collider.GetComponent<S_DestructibleWall>() != null && !processedColliders.Contains(hit.collider))
            {
                // Mark this collider as processed
                processedColliders.Add(hit.collider);

                // Increment the collision count
                collisionCount++;

                // Check if the collision count has reached the maximum value
                if (collisionCount >= maxCollisionCount)
                {
                    // Destroy this GameObject
                    Destroy(gameObject,0.5f);
                    break;
                }
            }
        }
    }

    // Draw Gizmos to visualize the BoxCast in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }

    // Debug to show BoxCast in the console
    void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, transform.forward * detectionDistance, Color.blue);
    }
}
