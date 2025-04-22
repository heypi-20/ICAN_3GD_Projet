using System;
using UnityEngine;

public class S_EnemyGroundCheck : MonoBehaviour
{
    #region Enums
    public enum OverlapType { BOX, SPHERE, CAPSULE }
    #endregion

    #region Public Settings
    [Header("Detection Settings")]
    public float physicCastCooldown = 1.0f;  // cooldown between checks

    [Header("OverlapType Settings")]
    public OverlapType overlapType = OverlapType.SPHERE;  // use sphere by default for ground check
    public bool withScaleSize = true;        // use object scale
    public bool ignoreChildren = false;      // skip child colliders

    [Header("Box Settings")]
    public Vector3 customSize = Vector3.one;
    public Vector3 customOffset = Vector3.zero;

    [Header("Sphere Settings")]
    public float sphereRadius = 1.0f;

    [Header("Capsule Settings")]
    public Vector3 capsulePoint1 = Vector3.zero;
    public Vector3 capsulePoint2 = Vector3.up;
    public float capsuleRadius = 1.0f;

    [Header("Check Condition")]
    public string[] physicCastTags;
    public LayerMask physicCastLayers = ~0;
    public string[] physicTargetComponentNames;
    #endregion

    #region Private Fields
    private float lastPhysicCastTime = -Mathf.Infinity;
    private Collider[] overlapBuffer = new Collider[1];  // small buffer for first hit only
    #endregion

    
    
    
    
    #region Public Method
    // call this to perform detection and get result
    public bool TriggerDetection()
    {
        if (Time.time < lastPhysicCastTime + physicCastCooldown) return false;
        lastPhysicCastTime = Time.time;
        return PerformPhysicCast();
    }
    #endregion

    #region Detection Methods
    // choose overlap method based on type
    private bool PerformPhysicCast()
    {
        switch (overlapType)
        {
            case OverlapType.BOX:     return PerformBox();
            case OverlapType.SPHERE:  return PerformSphere();
            case OverlapType.CAPSULE: return PerformCapsule();
            default: return false;
        }
    }

    // box overlap non-alloc
    private bool PerformBox()
    {
        Vector3 half = (withScaleSize ? transform.lossyScale : customSize) * 0.5f;
        Vector3 center = withScaleSize ? transform.position : transform.TransformPoint(customOffset);
        int count = Physics.OverlapBoxNonAlloc(center, half, overlapBuffer, transform.rotation, physicCastLayers);
        return ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
    }

    // sphere overlap non-alloc
    private bool PerformSphere()
    {
        Vector3 center = transform.TransformPoint(customOffset);
        float radius = withScaleSize
            ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 0.5f
            : sphereRadius;
        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, physicCastLayers);
        return ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
    }

    // capsule overlap non-alloc
    private bool PerformCapsule()
    {
        Vector3 p1 = transform.TransformPoint(capsulePoint1 + customOffset);
        Vector3 p2 = transform.TransformPoint(capsulePoint2 + customOffset);
        float radius = withScaleSize
            ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 0.5f
            : capsuleRadius;
        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, radius, overlapBuffer, physicCastLayers);
        return ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
    }
    #endregion

    #region Hit Processing
    // check hits against tags and component names
    private bool ProcessHits(Collider[] hits, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Collider hit = hits[i];  // only first hit considered
            if (hit.transform == transform) continue;
            if (ignoreChildren && hit.transform.IsChildOf(transform)) continue;
            if (CheckConditions(hit.gameObject, physicCastTags, physicTargetComponentNames))
                return true;
        }
        return false;
    }
    #endregion

    #region Condition Checks
    // verify tag and component matches
    private bool CheckConditions(GameObject obj, string[] tags, string[] components)
    {
        bool tagOk = tags == null || tags.Length == 0 || CheckTags(obj, tags);
        bool compOk = components == null || components.Length == 0 || CheckComponents(obj, components);
        return tagOk && compOk;
    }

    private bool CheckTags(GameObject obj, string[] tags)
    {
        foreach (var t in tags)
            if (obj.CompareTag(t)) return true;
        return false;
    }

    private bool CheckComponents(GameObject obj, string[] components)
    {
        foreach (var name in components)
            if (obj.GetComponent(name) != null) return true;
        return false;
    }
    #endregion

    #region Gizmos
    // visualize detection area in editor, yellow when detection hits
    private void OnDrawGizmosSelected()
    {
        // determine if currently overlapping (no cooldown)
        bool detected = false;
        switch (overlapType)
        {
            case OverlapType.BOX:
            {
                Vector3 half = (withScaleSize ? transform.lossyScale : customSize) * 0.5f;
                Vector3 center = withScaleSize ? transform.position : transform.TransformPoint(customOffset);
                int count = Physics.OverlapBoxNonAlloc(center, half, overlapBuffer, transform.rotation, physicCastLayers);
                detected = ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
                Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, half * 2);
                break;
            }
            case OverlapType.SPHERE:
            {
                Vector3 center = transform.TransformPoint(customOffset);
                float radius = withScaleSize
                    ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 0.5f
                    : sphereRadius;
                int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer, physicCastLayers);
                detected = ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
                Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
                Gizmos.DrawWireSphere(Vector3.zero, radius);
                break;
            }
            case OverlapType.CAPSULE:
            {
                Vector3 p1 = transform.TransformPoint(capsulePoint1 + customOffset);
                Vector3 p2 = transform.TransformPoint(capsulePoint2 + customOffset);
                float radius = withScaleSize
                    ? Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 0.5f
                    : capsuleRadius;
                int count = Physics.OverlapCapsuleNonAlloc(p1, p2, radius, overlapBuffer, physicCastLayers);
                detected = ProcessHits(overlapBuffer, Mathf.Min(count, overlapBuffer.Length));
                // draw capsules manually
                Gizmos.DrawWireSphere(p1, radius);
                Gizmos.DrawWireSphere(p2, radius);
                Gizmos.DrawLine(p1 + Vector3.up * radius, p2 + Vector3.up * radius);
                Gizmos.DrawLine(p1 - Vector3.up * radius, p2 - Vector3.up * radius);
                Gizmos.matrix = Matrix4x4.identity;
                break;
            }
        }
        Gizmos.color = detected ? Color.yellow : Color.blue;
        // draw shape after setting color and matrix
        if (overlapType == OverlapType.BOX)
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        else if (overlapType == OverlapType.SPHERE || overlapType == OverlapType.CAPSULE)
        {
            // already drawn sphere/capsule outlines above
        }
    }
    #endregion
}