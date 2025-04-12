using UnityEngine;
using Random = UnityEngine.Random;
public class S_JumpyCuby_Behavior : EnemyBase
{
    [Header("Jump Settings")]
    public float jumpForce = 10f; // The upward force applied when jumping
    public float trackingForce = 5f; // The additional force applied to track a target
    public float rotationSpeed = 1f;  
    [Header("Tracking Settings")]
    public bool enableTracking = false; // Whether the cube should track a target
    public Transform target; // The target to track (if tracking is enabled)

    [Header("Idle Activation Settings")]
    public float idleThreshold = 5f; // Time in seconds before activating
    public float activationForce = 5f; // Force to activate movement

    private Rigidbody rb;
    private float lastMovementTime;

    private void Start()
    {
        Cubyinitialize();
        lastMovementTime = Time.time;
    }

    private void Cubyinitialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on this GameObject!");
        }
        S_CustomCharacterController playerShooting = FindObjectOfType<S_CustomCharacterController>();
        if (playerShooting != null)
        {
            target = playerShooting.transform;
        }
        else
        {
            Debug.LogWarning("No object with S_PlayerShooting found in the scene!");
        }
    }

    private void Update()
    {
        CheckIdleState();
        Quaternion startRot = transform.rotation;
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion endRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(startRot, endRot, rotationSpeed*Time.deltaTime);
    }




    // Method to make the cube jump
    public void Jump()
    {
        if (rb == null) return;

        // Apply upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        SoundManager.Instance.Meth_JumpyCuby_Jump();
        
        // Apply tracking or random force
        if (enableTracking && target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            rb.AddForce(directionToTarget * trackingForce, ForceMode.Impulse);
        }
        else
        {
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDirection * trackingForce, ForceMode.Impulse);
        }

        lastMovementTime = Time.time; // Reset idle timer on jump
    }

    private void CheckIdleState()
    {
        // Check if the object has been idle for too long
        if (Time.time - lastMovementTime > idleThreshold&&rb.velocity.magnitude<=0.1)
        {
            ActivateMovement();
            lastMovementTime = Time.time; // Reset the timer
        }
    }

    private void ActivateMovement()
    {
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            1f, // Always add upward force
            Random.Range(-1f, 1f)).normalized;

        rb.AddForce(randomDirection * activationForce, ForceMode.Impulse);
    }
}
