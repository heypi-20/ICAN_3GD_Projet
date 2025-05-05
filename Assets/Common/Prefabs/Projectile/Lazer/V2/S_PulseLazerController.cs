using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PulseLazerController : MonoBehaviour
{
    public Material laserMat;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float speed = 1f;
    public float minStrength = 0.2f;
    public float maxStrength = 2f;

    void Update()
    {
        float t = Mathf.Repeat(Time.time * speed, 1f); // boucle entre 0 et 1
        float value = pulseCurve.Evaluate(t);          // valeur entre 0 et 1
        float strength = Mathf.Lerp(minStrength, maxStrength, value);
        laserMat.SetFloat("_EmissionStrength", strength);
    }
}
