
using UnityEngine;

public class S_OrbitAround : MonoBehaviour
{
    [Tooltip("The Transform to orbit around")] 
    public Transform target;

    [Tooltip("Orbit radius from the target")] 
    public float distance = 5f;

    [Tooltip("Orbit speed in degrees per second")] 
    public float orbitSpeed = 30f;

    [Tooltip("Axis of rotation (default Y axis)")] 
    public Vector3 axis = Vector3.up;

    // Current angle around the target, in degrees
    private float currentAngle = 0f;

    void Reset()
    {
        // If no target is set, try to find a GameObject named "Target"
        if (target == null)
        {
            var go = GameObject.Find("Target");
            if (go != null) target = go.transform;
        }
    }

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("OrbitAround: Target is not assigned!");
            enabled = false;
            return;
        }

        // Initialize with a random starting angle
        currentAngle = Random.Range(0f, 360f);
        UpdatePosition();
    }

    void Update()
    {
        if (target == null) return;

        // Increment the angle based on orbitSpeed and elapsed time
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        // Convert angle to radians
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 direction;

        // If rotating around Y axis, use XZ plane
        if (axis == Vector3.up)
        {
            direction = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
        }
        else
        {
            // For arbitrary axis, rotate the forward vector
            direction = Quaternion.AngleAxis(currentAngle, axis.normalized) * Vector3.forward;
        }

        // Position the object at the correct orbit radius
        transform.position = target.position + direction * distance;
    }
}
