using System.Collections.Generic;
using UnityEngine;

public class EyeOrbitController : MonoBehaviour
{
    [Header("Prefabs & Count")]
    public GameObject eyePrefab;
    [Range(0, 100)] public int eyeCount;
    private int previousEyeCount = -1;

    [Header("Central Eye Setup")]
    public GameObject centralEyePrefab;
    public GameObject haloPrefab;
    [Range(0.1f, 10f)] public float centralEyeScale = 1f;
    [Range(0.1f, 10f)] public float haloScale = 1f;

    [Header("Orbit Settings")]
    public float orbitRadius = 2f;
    public float rotationSpeed = 30f;
    public bool rotateClockwise = true;
    public Axis rotationAxis = Axis.Z;

    public enum Axis { X, Y, Z }

    [Header("Oscillation (optionnel)")]
    public bool enableOscillation = false;
    public float oscillationAmplitude = 0.5f;
    public float oscillationSpeed = 2f;

    private GameObject centralEyeObject;
    private GameObject haloObject;
    private List<GameObject> eyes = new List<GameObject>();
    private float currentAngleOffset = 0f;
    private float timeElapsed = 0f;

    void Start()
    {
        InitCentralVisuals();
        CreateEyes();
        previousEyeCount = eyeCount;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        // Si la valeur a changé depuis la frame précédente, on met à jour
        if (eyeCount != previousEyeCount)
        {
            int delta = eyeCount - previousEyeCount;

            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                    AddEye();
            }
            else
            {
                for (int i = 0; i < -delta; i++)
                    RemoveEye();
            }

            previousEyeCount = eyeCount;
        }

        RotateEyes();
        UpdateCentralScales();
    }

    void InitCentralVisuals()
    {
        // Nettoyage si déjà instancié
        if (centralEyeObject != null)
            Destroy(centralEyeObject);

        if (haloObject != null)
            Destroy(haloObject);

        if (centralEyePrefab != null)
        {
            centralEyeObject = Instantiate(centralEyePrefab, transform);
            centralEyeObject.name = "CentralEye";
            centralEyeObject.transform.localPosition = Vector3.zero;
        }

        if (haloPrefab != null)
        {
            haloObject = Instantiate(haloPrefab, transform);
            haloObject.name = "Halo";
            haloObject.transform.localPosition = Vector3.zero;
        }
    }

    void UpdateCentralScales()
    {
        if (centralEyeObject != null)
            centralEyeObject.transform.localScale = Vector3.one * centralEyeScale;

        if (haloObject != null)
            haloObject.transform.localScale = Vector3.one * haloScale;
    }

    public void CreateEyes()
    {
        ClearEyes();

        if (eyePrefab == null) return;

        for (int i = 0; i < eyeCount; i++)
        {
            GameObject newEye = Instantiate(eyePrefab, transform);
            newEye.name = "OrbitingEye_" + i;
            eyes.Add(newEye);
        }

        PositionEyes();
    }

    public void ClearEyes()
    {
        foreach (GameObject eye in eyes)
        {
            if (eye != null)
                Destroy(eye);
        }
        eyes.Clear();
    }

    public void UpdateEyeCount(int newCount)
    {
        eyeCount = Mathf.Max(0, newCount);
        CreateEyes();
    }

    public void AddEye()
    {
        eyeCount++;
        CreateEyes();
    }

    public void RemoveEye()
    {
        eyeCount = Mathf.Max(0, eyeCount - 1);
        CreateEyes();
    }

    void RotateEyes()
    {
        float direction = rotateClockwise ? -1f : 1f;
        currentAngleOffset += direction * rotationSpeed * Time.deltaTime;
        PositionEyes();
    }

    void PositionEyes()
    {
        if (eyes.Count == 0) return;

        float angleStep = 360f / eyes.Count;

        for (int i = 0; i < eyes.Count; i++)
        {
            if (eyes[i] == null) continue;

            float angle = angleStep * i + currentAngleOffset;
            float radius = orbitRadius;

            if (enableOscillation)
            {
                radius += Mathf.Sin(timeElapsed * oscillationSpeed + i) * oscillationAmplitude;
            }

            Vector3 offset = GetOrbitOffset(angle, radius);
            eyes[i].transform.localPosition = offset;
        }
    }

    Vector3 GetOrbitOffset(float angleDeg, float radius)
    {
        float rad = angleDeg * Mathf.Deg2Rad;

        switch (rotationAxis)
        {
            case Axis.X: return new Vector3(0, Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            case Axis.Y: return new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            case Axis.Z:
            default: return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
        }
    }
}


