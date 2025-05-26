using System;
using UnityEngine;

public class S_TutorialEnergy : MonoBehaviour
{
    public float energy;

    private S_EnergyStorage eStorage;

    private void Start()
    {
        eStorage = FindObjectOfType<S_EnergyStorage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && eStorage.currentEnergy != energy) {
            eStorage.currentEnergy = energy;
        }
    }
}

