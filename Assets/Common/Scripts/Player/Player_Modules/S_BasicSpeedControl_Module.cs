using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_CustomCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
[Serializable]
public class SpeedLevel
{
    public int level; // Correspond au niveau d'énergie dans S_EnergyStorage
    public float speed; // Vitesse associée à ce niveau
    public float energyConsumptionRate; // Consommation d'énergie associée à ce niveau
}
public class S_BasicSpeedControl_Module : MonoBehaviour
{
    

    [Header("Speed and Energy Settings")]
    public List<SpeedLevel> speedLevels; // Liste des niveaux de vitesse et consommation

    // Composant pour ajuster la vitesse de déplacement
    private S_CustomCharacterController _characterController;
    // Composant pour vérifier et modifier l'énergie disponible
    private S_EnergyStorage _energyStorage;
    // Référence au module de sprint pour vérifier l'état du sprint
    private S_BasicSprint_Module _basicSprintModule;

    private void Start()
    {
        // Initialisation des composants nécessaires
        _characterController = GetComponent<S_CustomCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _basicSprintModule = GetComponent<S_BasicSprint_Module>();
    }

    private void Update()
    {
        if (!IsSprinting())
        {
            UpdateSpeedAndConsumption();
        }
    }

    // Vérifie si le joueur est en train de sprinter pour éviter de modifier la vitesse normale
    private bool IsSprinting()
    {
        return _basicSprintModule != null && (_basicSprintModule._isSprinting || _basicSprintModule.IsSprintCoroutineRunning());
    }

    private float totalenergy;
    // Met à jour la vitesse et la consommation en fonction du niveau d'énergie actuel
    private void UpdateSpeedAndConsumption()
    {
        if (_energyStorage != null && _characterController != null)
        {
            int currentLevel = _energyStorage.currentLevelIndex + 1; // Ajuste pour correspondre au niveau dans SpeedLevel

            // Trouve le niveau correspondant dans speedLevels
            SpeedLevel level = speedLevels.Find(l => l.level == currentLevel);

            if (level != null)
            {
                // Met à jour la vitesse
                _characterController.moveSpeed = level.speed;

                // Consomme de l'énergie si le joueur se déplace
                if (_characterController._inputDirection.magnitude > 0)
                {
                    float energyToConsume = level.energyConsumptionRate * Time.deltaTime;
                    _energyStorage.currentEnergy -= energyToConsume;
                    totalenergy += energyToConsume;
                    Debug.Log("Energy"+totalenergy);
                }
            }
            else
            {
                Debug.LogWarning($"Aucun niveau trouvé pour le niveau actuel {currentLevel}.");
            }
        }
    }
}
