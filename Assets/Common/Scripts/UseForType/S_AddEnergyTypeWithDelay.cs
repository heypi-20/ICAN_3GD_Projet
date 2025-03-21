using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AddEnergyTypeWithDelay : MonoBehaviour
{
    public float delay = 1f;
    public float energyGiven;
    public float destructionTime=10f;

    void Start()
    {
        // Schedule the AddScript method to be called after a delay.
        Invoke(nameof(AddScript), delay);
    }

    void AddScript()
    {
        
        EnergyType energyType = gameObject.AddComponent<EnergyType>();
        energyType.energyGivenUseForType=energyGiven;
        energyType.destructionTimeUseForType=destructionTime;
    }

}
