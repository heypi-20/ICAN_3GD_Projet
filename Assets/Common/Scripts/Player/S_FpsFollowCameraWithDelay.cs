using UnityEngine;

public class S_FpsDynamicFollow : MonoBehaviour
{
    [Header("Follow Parameters")]
    public float positionLerpSpeed = 15f;
    public float rotationLerpSpeed = 5f;
    public float rotationFactor = 0.02f;
    public float positionFactor = 0.02f;

    // Store initial local state of the hand
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    // Accumulated mouse input
    private float accumulatedMouseX;
    private float accumulatedMouseY;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // Get mouse delta
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Accumulate mouse data (could also use current frame data directly; this is for demonstration)
        accumulatedMouseX += mouseX;
        accumulatedMouseY += mouseY;

        // Calculate hand offset; adjust these values and factors according to your project
        // Rotation offset follows the mouse input direction
        Quaternion targetRotation = Quaternion.Euler(accumulatedMouseY * rotationFactor, accumulatedMouseX * rotationFactor, 0) * initialLocalRotation;

        // Position offset is in the opposite direction of the mouse input (e.g., mouse moves right, hand moves left)
        Vector3 targetPosition = initialLocalPosition + new Vector3(-accumulatedMouseX * positionFactor, -accumulatedMouseY * positionFactor, 0);

        // Smooth transition to the target rotation and position
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, positionLerpSpeed * Time.deltaTime);

        // Optionally, when the mouse stops, let the offset naturally return by applying decay logic to gradually reduce the accumulated values
        accumulatedMouseX = Mathf.Lerp(accumulatedMouseX, 0, positionLerpSpeed * Time.deltaTime);
        accumulatedMouseY = Mathf.Lerp(accumulatedMouseY, 0, positionLerpSpeed * Time.deltaTime);
    }
}
