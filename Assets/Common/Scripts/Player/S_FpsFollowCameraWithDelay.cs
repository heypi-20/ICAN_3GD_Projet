using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FpsFollowWithDelay : MonoBehaviour
{
    [Header("摄像机引用")]
    public Transform cameraTransform; // 在 Inspector 中分配你的摄像机
    public Transform targetTransform;

    [Header("位置偏移")]
    public Vector3 positionOffset = new Vector3(0, 0, 0); // 相对于摄像机中心的偏移

    [Header("旋转偏移（欧拉角）")]
    public Vector3 rotationOffset = new Vector3(0, 0, 0); // 相对于摄像机旋转的附加偏移

    [Header("旋转平滑时间（延迟效果）")]
    public float rotationSmoothTime = 0.1f; // 数值越大旋转延迟越明显

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("摄像机引用未分配！");
            return;
        }

        // 1. 设置位置：始终以摄像机中心为基准，再加上你设定的局部偏移
        Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
        targetTransform.position = targetPosition;

        // 2. 计算目标旋转：以摄像机的旋转为基础，再加上旋转偏移
        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(rotationOffset);

        // 3. 使用 Slerp 平滑旋转：这样可以加一点延迟，使手部看起来更生动，同时减少抖动
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