using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEffect : MonoBehaviour
{
    private Material objectMaterial;
    public Color mycolor = Color.white;

    public float maxEmissionIntensity = 20f;
    public float minEmissionIntensity = 5f;
    private float currentIntensity;

    private static readonly string EmissionColorProperty = "_EmissionColor";

    public GameObject RingA;
    public GameObject RingB;
    public int Speed;
    private float currentRotationSpeed = 0f;
    public float rotationLerpSpeed = 5f;

    public S_PlayerStateObserver playerStateObserver;

    void Start()
    {
        objectMaterial = GetComponent<Renderer>().material;
        currentIntensity = minEmissionIntensity;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.K))
        {
            currentIntensity = Mathf.Lerp(currentIntensity, maxEmissionIntensity, Time.deltaTime * 5f);
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, Speed, Time.deltaTime * rotationLerpSpeed);
        }
        else
        {
            currentIntensity = Mathf.Lerp(currentIntensity, minEmissionIntensity, Time.deltaTime * 5f);
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0f, Time.deltaTime * rotationLerpSpeed);
        }

        // Appliquer la rotation fluide
        RingA.transform.Rotate(0, 0, currentRotationSpeed * Time.deltaTime);
        RingB.transform.Rotate(0, 0, -currentRotationSpeed * Time.deltaTime);

        // Appliquer l'émissive
        objectMaterial.SetColor(EmissionColorProperty, mycolor * currentIntensity);
    }
}
