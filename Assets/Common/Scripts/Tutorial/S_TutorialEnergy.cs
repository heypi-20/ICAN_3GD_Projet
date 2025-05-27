using System;
using UnityEngine;

public class S_TutorialEnergy : MonoBehaviour
{
    public float energy;

    private S_EnergyStorage eStorage;
    public bool Tuto2 = false;
    private void Start()
    {
        eStorage = FindObjectOfType<S_EnergyStorage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && eStorage.currentEnergy != energy) {
            eStorage.currentEnergy = energy;
        }
        if(Tuto2 == true)
        {
            other.GetComponent<S_SuperJump_Module>().enabled = false;
        }
        else
        {
            other.GetComponent<S_SuperJump_Module>().enabled = true;
        }
        Destroy(this.gameObject);
    }
}

