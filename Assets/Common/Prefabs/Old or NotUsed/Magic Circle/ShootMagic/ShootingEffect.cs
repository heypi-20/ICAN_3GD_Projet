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
    public GameObject RingC;
    public int Speed;
    private float currentRotationSpeed = 0f;
    public float rotationLerpSpeed = 5f;

    public S_PlayerStateObserver playerStateObserver;

    public bool RingAOn;
    public bool RingBOn;
    public bool RingCOn;

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
        if(RingAOn)
        {
            RingA.transform.Rotate(0, currentRotationSpeed * Time.deltaTime, 0);
        }
        if (RingBOn)
        {
            RingB.transform.Rotate(0, -currentRotationSpeed * Time.deltaTime, 0);
        }
        if (RingCOn)
        {
            RingC.transform.Rotate(0, currentRotationSpeed * Time.deltaTime, 0);
        }

        // Appliquer l'émissive
        objectMaterial.SetColor(EmissionColorProperty, mycolor * currentIntensity);
    }
}
