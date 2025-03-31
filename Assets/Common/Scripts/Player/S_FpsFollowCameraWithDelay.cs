using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FpsFollowWithDelay : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform; // Assign your camera in the Inspector
    public Transform targetTransform;

    [Header("Position Offset")]
    public Vector3 positionOffset = new Vector3(0, 0, 0); // Offset relative to the camera's center

    [Header("Rotation Offset (Euler angles)")]
    public Vector3 rotationOffset = new Vector3(0, 0, 0); // Additional rotation offset relative to the camera's rotation

    [Header("Rotation Smooth Time (Delay Effect)")]
    public float rotationSmoothTime = 0.1f; // Higher values create a more noticeable rotation delay

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera reference is not assigned!");
            return;
        }

        // 1. Set the position: always based on the camera's center plus the specified local offset
        Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
        targetTransform.position = targetPosition;

        // 2. Calculate the target rotation: based on the camera's rotation plus the additional rotation offset
        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);

        // 3. Use Slerp for smooth rotation: adding a delay effect to make the weapon (or hands) look more natural and reduce jitter
        if (rotationSmoothTime > 0)
        {
            targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, targetRotation, Time.deltaTime / rotationSmoothTime);
        }
        else
        {
            targetTransform.rotation = targetRotation;
        }
    }
}