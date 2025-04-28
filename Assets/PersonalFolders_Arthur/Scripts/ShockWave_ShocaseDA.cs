using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave_ShocaseDA : MonoBehaviour
{
    private float Timer;
    private float Distance;
    public float MultiplierDistance = 1f;
    public float Time_To_Reset = 3f;

    private void Update()
    {
        Timer += Time.deltaTime;
        Distance += Time.deltaTime * MultiplierDistance;

        if (Timer >=  Time_To_Reset)
        {
            Distance = 0f;
            Timer = 0f;
        }
        
        GetComponent<Renderer>().material.SetFloat("_Distance", Distance);
    }
}
