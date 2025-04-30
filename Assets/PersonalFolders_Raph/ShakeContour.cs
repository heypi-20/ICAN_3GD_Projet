using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeContour : MonoBehaviour
{
    public Material waveMat;
    public string scaleProperty = "_WaveScale";
    public float baseScale = 1.5f;
    public float amplitude = 0.5f;
    public float speed = 2.0f;

    void Update()
    {
        float scale = baseScale + Mathf.Sin(Time.time * speed) * amplitude;
        waveMat.SetFloat(scaleProperty, scale);
    }
}