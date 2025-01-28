using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyType : MonoBehaviour
{
    public float energyGiven;
    public float destructionTime=10f;

    private void Start()
    {
        Destroy(gameObject, destructionTime);
    }
}
