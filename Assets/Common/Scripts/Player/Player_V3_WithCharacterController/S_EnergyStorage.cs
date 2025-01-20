using System;
using UnityEngine;
using TMPro;

[Serializable]
public class EnergyLevel
{
    public int level; // Énergie actuelle du niveau
    public float requiredEnergy; // Énergie requise pour atteindre ce niveau
    public float graceTimer; // Temps de grâce pour maintenir le niveau si l'énergie est insuffisante
}

public class S_EnergyStorage : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 2000f; // Énergie maximale
    public float currentEnergy = 0f; // Énergie actuelle

    [Header("Energy Levels")]
    public EnergyLevel[] energyLevels; // Liste des niveaux d'énergie configurables

    [Header("UI Settings")]
    public TextMeshProUGUI energyDisplay; // Affichage de l'énergie et du niveau

    private int currentLevelIndex = 0; // Index du niveau actuel
    private float _graceTimer = 0f; // Temps de grâce restant
    private bool isGraceActive = false; // Indique si le temps de grâce est en cours
    private bool hasDeathTriggered = false; // Indique si la logique de mort a été déclenchée

    private void Update()
    {
        UpdateEnergyLevel();
        UpdateEnergyDisplay();
    }

    // Méthode permettant d'ajouter de l'énergie
    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, -Mathf.Infinity, maxEnergy);

        // Si l'énergie redevient positive après la mort, réinitialise l'état
        if (hasDeathTriggered && currentEnergy > 0f)
        {
            ResetDeathState();
        }
    }

    // Méthode permettant de retirer de l'énergie
    public void RemoveEnergy(float amount)
    {
        currentEnergy -= amount;
    }

    // Met à jour l'affichage combiné de l'énergie et du niveau
    private void UpdateEnergyDisplay()
    {
        if (energyDisplay != null)
        {
            energyDisplay.text = $"Niveau : {energyLevels[currentLevelIndex].level} | Énergie : {currentEnergy:F2}/{maxEnergy}";
        }
    }

    // Met à jour le niveau d'énergie en fonction de l'énergie actuelle
    private void UpdateEnergyLevel()
    {
        if (hasDeathTriggered) return;

        // Vérifie les conditions pour augmenter de niveau
        if (CheckUpgradeLevel())
        {
            return;
        }

        // Vérifie les conditions pour descendre de niveau
        CheckDowngradeLevel();
    }

    // Vérifie si le niveau doit être augmenté
    private bool CheckUpgradeLevel()
    {
        for (int i = energyLevels.Length - 1; i > currentLevelIndex; i--)
        {
            //L'énergie actuelle dépasse celle requise pour le niveau suivant.
            if (currentEnergy >= energyLevels[i].requiredEnergy)
            {
                SetNewLevel(i); // Définit le nouveau niveau
                return true;
            }
        }

        return false;
    }

    // Vérifie si le niveau doit être diminué
    private void CheckDowngradeLevel()
    {
        //Si l'énergie actuelle est inférieure au minimum requis pour le niveau actuel
        if (currentEnergy < energyLevels[currentLevelIndex].requiredEnergy)
        {
            //et que le compte à rebours n'est pas encore lancé, démarrez-le.
            if (!isGraceActive)
            {
                StartGracePeriod();
            }

            _graceTimer -= Time.deltaTime;
            
            if (_graceTimer <= 0f)
            {
                //Fin du compte à rebours : énergie négative, début du compte à rebours de mort.
                if (currentEnergy <= 0f)
                {
                    HandleDeath();
                }
                else
                {
                    //Fin du compte à rebours : énergie positive, ajustement au niveau correspondant.
                    int newLevelIndex = FindLevelIndexForEnergy();
                    SetNewLevel(newLevelIndex);
                }

                ResetGracePeriod();
            }
        }
        else
        {
            ResetGracePeriod();
        }
    }

    // Définit un nouveau niveau d'énergie
    private void SetNewLevel(int newLevelIndex)
    {
        currentLevelIndex = newLevelIndex;
        isGraceActive = false;
        _graceTimer = 0f;
    }

    // Démarre une période de grâce
    private void StartGracePeriod()
    {
        isGraceActive = true;
        _graceTimer = energyLevels[currentLevelIndex].graceTimer;
    }

    // Réinitialise la période de grâce
    private void ResetGracePeriod()
    {
        isGraceActive = false;
        _graceTimer = 0f;
    }

    // Trouve l'index du niveau correspondant à l'énergie actuelle
    private int FindLevelIndexForEnergy()
    {
        for (int i = energyLevels.Length - 1; i >= 0; i--)
        {
            if (currentEnergy >= energyLevels[i].requiredEnergy)
            {
                return i;
            }
        }

        return 0; // Si aucune correspondance, retourne le niveau le plus bas
    }

    // Logique de gestion de la mort
    private void HandleDeath()
    {
        Debug.Log("Le joueur est mort. Implémentez la logique ici.");
        GetComponent<S_CountdownVFX>().enabled = true;
        hasDeathTriggered = true;
    }

    // Réinitialise l'état de mort
    private void ResetDeathState()
    {
        hasDeathTriggered = false;
        ResetGracePeriod();
        Debug.Log("Réinitialisation après la mort.");
    }
}
