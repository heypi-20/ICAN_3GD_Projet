using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerLineRenderer : MonoBehaviour
{
    [Header("Line Setup")]
    public Transform startPoint;
    public float maxDistance = 5000f;
    public LayerMask targetLayers;

    [Header("Debug")]
    public bool showLaserIfNoHit = false;

    public LineRenderer lineRenderer;

    void Awake()
    {
        //lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.numCapVertices = 8;     // arrondit les extrémités
        lineRenderer.numCornerVertices = 8;
    }

    void Update()
    {
        if (startPoint == null) return;

        Vector3 origin = startPoint.position;
        Vector3 direction = startPoint.forward;

        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        bool hitSomething = Physics.Raycast(ray, out hit, maxDistance, targetLayers);

        if (hitSomething)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, startPoint.transform.position);
            lineRenderer.SetPosition(1, hit.point);
        }
        else if (showLaserIfNoHit)
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + direction * maxDistance);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
}
