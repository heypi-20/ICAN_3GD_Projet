using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_PlayerController : MonoBehaviour
{
    // Public variables for configuration
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 1.0f;
    public LayerMask groundMask;
    public float maxSlopeAngle = 45f;
    public bool enableSlopeSlide = true;
    public float gravity = 9.81f;
    public bool showGizmos = true;

    // Private variables
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    // Initialization
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Physics update
    private void FixedUpdate()
    {
        DetectGround();
        ApplyGravityAndSlopeHandling();
    }

    // Ground detection logic using SphereCast
    private void DetectGround()
    {
        Vector3 sphereCastOrigin = transform.position;
        Ray downRay = new Ray(sphereCastOrigin, Vector3.down);

        if (Physics.SphereCast(downRay, groundCheckRadius, out RaycastHit hitInfo, groundCheckDistance, groundMask))
        {
            isGrounded = true;
            groundNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    // Applies gravity and handles slope sliding if necessary
    private void ApplyGravityAndSlopeHandling()
    {
        float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);

        if (isGrounded)
        {
            if (enableSlopeSlide && slopeAngle > maxSlopeAngle)
            {
                Vector3 slideDirection = Vector3.Cross(groundNormal, Vector3.Cross(groundNormal, Vector3.up)).normalized;
                rb.AddForce(slideDirection * gravity, ForceMode.Acceleration);
            }
        }
        else
        {
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        }
    }

    // Visualizes the SphereCast using Gizmos
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Vector3 sphereCastOrigin = transform.position;
        Vector3 sphereCastEnd = sphereCastOrigin + Vector3.down * groundCheckDistance;

        // Draw the sphere at the end of the SphereCast
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(sphereCastEnd, groundCheckRadius);

        // Draw the line representing the SphereCast distance
        Gizmos.DrawLine(sphereCastOrigin, sphereCastEnd);
    }
}
