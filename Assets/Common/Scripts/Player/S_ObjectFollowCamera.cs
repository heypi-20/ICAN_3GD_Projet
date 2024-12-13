using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ObjectFollowCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform cameraTransform; // Reference to the camera's transform
    public Vector3 offset = Vector3.zero; // Offset from the camera's position
    public bool matchRotation = true; // Whether the object should match the camera's rotation

    private void LateUpdate()
    {
        FollowCamera();
    }

    private void FollowCamera()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("Camera Transform is not assigned!");
            return;
        }

        // Set the position relative to the camera with offset
        transform.position = cameraTransform.position + offset;

        // Match the rotation if enabled
        if (matchRotation)
        {
            transform.rotation = cameraTransform.rotation;
        }
    }
}
