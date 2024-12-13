using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GunFollowCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera mainCamera; // Reference to the main camera

    [Header("Center Point Settings")]
    public float distanceFromCamera = 1f; // Distance from the camera to the center point
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

        // Align the position of the object to the calculated center point
        transform.position = centerPoint;

        // Align the rotation of the object to match the camera's rotation, with an optional offset
        transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(rotationOffset);
    }
}
