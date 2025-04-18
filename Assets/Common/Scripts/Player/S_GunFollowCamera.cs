using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GunFollowCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera mainCamera; // Reference to the main camera

    [Header("Offset Settings")]
    public float distanceFromCamera = 1f; // Distance from the camera to the center point
    public Vector3 positionOffset = Vector3.zero; // Positional offset relative to the calculated position
    public Vector3 rotationOffset = Vector3.zero; // Rotational offset relative to the camera

    [Header("Delay Follow Options")]
    public bool useDelayFollow = false; // Toggle to enable/disable delayed following
    public float followDelay = 0.1f; // Delay time for position smoothing (set a small value for subtle effect)
    public float rotationDelay = 0.1f; // Delay time for rotation smoothing

    private void LateUpdate()
    {
        AlignWithCameraCenter();
    }

    private void AlignWithCameraCenter()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned!");
            return;
        }

        // Calculate the center point in front of the camera
        Vector3 centerPoint = mainCamera.transform.position + mainCamera.transform.forward * distanceFromCamera;

        // Apply the positional offset
        Vector3 targetPosition = centerPoint + mainCamera.transform.right * positionOffset.x
                                               + mainCamera.transform.up * positionOffset.y
                                               + mainCamera.transform.forward * positionOffset.z;

        // Calculate the target rotation based on the camera rotation and additional offset
        Quaternion targetRotation = mainCamera.transform.rotation * Quaternion.Euler(rotationOffset);

        if (useDelayFollow)
        {
            // Smoothly transition to the target position and rotation
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime / followDelay);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationDelay);
        }
        else
        {
            // Directly assign the calculated position and rotation
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
}
