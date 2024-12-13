using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_GunFollowCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform; // Reference to the camera

    [Header("Center Point Settings")]
    public float distanceFromCamera = 1f; // Distance from the camera to the center point
    public Vector3 rotationOffset = Vector3.zero; // Rotational offset relative to the camera

    private void LateUpdate()
    {
        AlignWithCameraCenter();
    }

    private void AlignWithCameraCenter()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform is not assigned!");
            return;
        }

        // Calculate the position at the center point in front of the camera
        transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;

        // Align rotation to match the camera, with optional rotation offset
        transform.rotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
    }
}
