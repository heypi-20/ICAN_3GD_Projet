using System;
using UnityEngine;
using TMPro;

public class S_EnergyStorage : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 2000f;
    public float currentEnergy = 0f;

    [Header("UI Settings")]
    public TextMeshProUGUI energyDisplay;

    // Met à jour l'affichage de l'énergie à chaque frame
    private void Update()
    {
        UpdateEnergyDisplay();
    }

    // Méthode permettant d'ajouter de l'énergie à la réserve
    public void AddEnergy(float amount)
    {
        // Ajoute l'énergie tout en s'assurant que la valeur reste entre -Infinity et maxEnergy
        currentEnergy = Mathf.Clamp(currentEnergy + amount, -Mathf.Infinity, maxEnergy);
    }

    // Méthode permettant de retirer de l'énergie de la réserve
    public void RemoveEnergy(float amount)
    {
        currentEnergy -= amount;
    }

    // Met à jour l'interface utilisateur pour afficher la quantité d'énergie actuelle
    private void UpdateEnergyDisplay()
    {
        if (energyDisplay != null)
        {
            // Met le texte à jour avec la valeur actuelle d'énergie formatée avec deux décimales
            energyDisplay.text = $"Energy: {currentEnergy:F2}/{maxEnergy}";
        }
    }
}