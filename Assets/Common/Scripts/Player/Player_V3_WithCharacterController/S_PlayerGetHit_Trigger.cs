using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerGetHit_Trigger : MonoBehaviour
{
    
    private S_EnergyStorage _energyStorage;


    private void Start()
    {
        _energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        _energyStorage.RemoveEnergy(other.gameObject.GetComponent<EnemyBase>().enemyDamage);
    }
}
