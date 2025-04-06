 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnergyType : MonoBehaviour
{
    public float energyGivenUseForType;
    public float destructionTimeUseForType = 10f;

    private float timer;

    private void OnEnable()
    {
        timer = destructionTimeUseForType;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            TryReturnToPoolOrDestroy();
        }
    }

    private void TryReturnToPoolOrDestroy()
    {
        var addScript = GetComponent<S_AddEnergyTypeWithDelay>();
        if (addScript != null && addScript.selfPrefab != null)
        {
            S_EnergyPointPoolManager.Instance?.ReturnToPool(gameObject, addScript.selfPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
