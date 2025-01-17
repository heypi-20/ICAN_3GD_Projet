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
        Vector3 finalPosition = centerPoint + mainCamera.transform.right * positionOffset.x
                                            + mainCamera.transform.up * positionOffset.y
                                            + mainCamera.transform.forward * positionOffset.z;

        // Align the position of the object to the calculated center point with the offset
        transform.position = finalPosition;

        // Align the rotation of the object to match the camera's rotation, with an optional offset
        transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(rotationOffset);
    }
}