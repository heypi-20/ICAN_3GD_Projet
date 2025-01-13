using System;
using UnityEngine;
using TMPro;

public class S_EnergyStorage : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 2000f; // Maximum energy capacity
    public float currentEnergy = 0f; // Current stored energy

    [Header("UI Settings")]
    public TextMeshProUGUI energyDisplay; // TextMeshPro to display energy value

    private void Start()
    {
        UpdateEnergyDisplay();
    }

    private void Update()
    {
        UpdateEnergyDisplay();
    }

    // Method to add energy
    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, -Mathf.Infinity, maxEnergy);
    }

    // Method to remove energy
    public void RemoveEnergy(float amount)
    {
        currentEnergy -= amount;
    }

    // Updates the UI to display the current energy
    private void UpdateEnergyDisplay()
    {
        if (energyDisplay != null)
        {
            energyDisplay.text = $"Energy: {currentEnergy:F2}/{maxEnergy}";
        }
    }
}