using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_HandRecoil : MonoBehaviour
{
    public Transform mainCamera;         // Reference to the main camera
    public float offsetMultiplier = 0.1f;  // Scale factor for the offset
    public float returnSpeed = 5f;         // Speed at which the hand returns to the original position

    private Vector3 currentOffset = Vector3.zero;
    private Vector3 targetOffset = Vector3.zero;
    private Vector3 lastCameraEuler;

    void Start() {
        // Record the initial rotation of the camera
        lastCameraEuler = mainCamera.eulerAngles;
    }

    void LateUpdate() {
        // Get the current rotation of the camera
        Vector3 currentCameraEuler = mainCamera.eulerAngles;
        
        // Calculate the rotation difference (handling angle wrap-around)
        float deltaX = Mathf.DeltaAngle(lastCameraEuler.x, currentCameraEuler.x);
        float deltaY = Mathf.DeltaAngle(lastCameraEuler.y, currentCameraEuler.y);
        
        // Calculate the target offset based on the rotation difference (swapping the mapping of X and Y)
        targetOffset = new Vector3(-deltaY * offsetMultiplier, -deltaX * offsetMultiplier, 0);
        
        // Smoothly transition to the target offset
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * returnSpeed);
        
        // Apply the offset
        transform.localPosition = currentOffset;
        
        // Update the last recorded camera rotation
        lastCameraEuler = currentCameraEuler;
    }
}