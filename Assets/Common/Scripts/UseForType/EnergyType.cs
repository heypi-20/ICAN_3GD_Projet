 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnergyType : MonoBehaviour
{
    public float energyGivenUseForType;
    public float destructionTimeUseForType=10f;

    private void Start()
    {
        Destroy(gameObject, destructionTimeUseForType);
    }
}
