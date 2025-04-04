using UnityEngine;

public class S_GizmosForSpawnerPoint : MonoBehaviour
{
    public enum GizmoShape { Sphere, Cube }

    [Header("Gizmo Settings")]
    public Color gizmoColor = Color.red;
    public float size = 1f;
    public GizmoShape shape = GizmoShape.Sphere;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        switch (shape)
        {
            case GizmoShape.Sphere:
                Gizmos.DrawSphere(transform.position, size);
                break;
            case GizmoShape.Cube:
                Gizmos.DrawCube(transform.position, Vector3.one * size);
                break;
        }
    }
}