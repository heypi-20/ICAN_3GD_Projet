using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

[Serializable]
public class EnergyLevel
{
    public int level; // Énergie actuelle du niveau
    public float requiredEnergy; // Énergie requise pour atteindre ce niveau
    public float graceTimer; // Temps de grâce pour maintenir le niveau si l'énergie est insuffisante
    public float percentageGiveaway;
}

public class S_EnergyStorage : MonoBehaviour
{
    [Header("CheatCode")] 
    public KeyCode cheatCodeforAdd;
    public KeyCode cheatCodeforRemove;
    
    
    [Header("Energy Settings")]
    public float maxEnergy = 2000f; // Énergie maximale
    public float currentEnergy = 0f; // Énergie actuelle

    [Header("Energy Levels")]
    public EnergyLevel[] energyLevels; // Liste des niveaux d'énergie configurables

    [Header("UI Settings")]
    public TextMeshProUGUI energyDisplay; // Affichage de l'énergie et du niveau

    [Header("UI Gain Loose Palier")] 
    public GameObject Gain_Palier; //Ui quand on gagne un palier
    public GameObject Loose_Palier; //UI quand on perd un palier
    public float animationDuration = 0.5f; // Durée pour l'apparition/disparition
    public float activeTime = 2f;          // Temps pendant lequel l'objet reste actif
    public Vector3 active_scale = new Vector3(0.3f, 0.3f, 0.3f);

    public int currentLevelIndex{ get; private set; } // Index du niveau actuel
    private float _graceTimer = 0f; // Temps de grâce restant
    private bool isGraceActive = false; // Indique si le temps de grâce est en cours
    private bool hasDeathTriggered = false; // Indique si la logique de mort a été déclenchée
    
    public event Action<Enum,int> OnLevelChange; 
    private bool EndGraceEventSended;


    private void Start()
    {
        EndGraceEventSended = true;
    }

    private void Update()
    {
        UpdateEnergyLevel();
        UpdateEnergyDisplay();
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        CheatCode();
    }

    private void EnergyLevelObserverEvent(Enum LevelState,int Level)
    {
        OnLevelChange?.Invoke(LevelState, Level);
    }
    
    
    private void CheatCode()
    {
        if (Input.GetKeyUp(cheatCodeforAdd))
        {
            AddEnergy(1000f);
        }

        if (Input.GetKeyUp(cheatCodeforRemove))
        {
            RemoveEnergy(1000f);
        }
    }

    // Méthode permettant d'ajouter de l'énergie
    public void AddEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);

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
                SoundManager.Instance.Meth_Gain_Palier();
                AddEnergy(energyLevels[i].requiredEnergy*energyLevels[i].percentageGiveaway);
                //trigger upgrade level event
                EnergyLevelObserverEvent(PlayerStates.LevelState.LevelUp, i+1);
                
                ShowAndHideGainPalier();
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
                //Fin du compte à rebours : énergie positive, ajustement au niveau correspondant.
                int newLevelIndex = FindLevelIndexForEnergy();
                SoundManager.Instance.Meth_Lose_Palier();
                
                //Trigger down level event
                EnergyLevelObserverEvent(PlayerStates.LevelState.LevelDown, newLevelIndex+1);
                
                ShowAndHideLoosePalier();
               
                SetNewLevel(newLevelIndex);
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
        EnergyLevelObserverEvent(PlayerStates.LevelState.StartGrace, energyLevels[currentLevelIndex].level);
        EndGraceEventSended = false;
    }

    // Réinitialise la période de grâce
    private void ResetGracePeriod()
    {
        isGraceActive = false;
        _graceTimer = 0f;

        if (EndGraceEventSended == false)
        {
            EnergyLevelObserverEvent(PlayerStates.LevelState.EndGrace, energyLevels[currentLevelIndex].level);
            EndGraceEventSended = true;
        }
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
        hasDeathTriggered = true;
    }

    // Réinitialise l'état de mort
    private void ResetDeathState()
    {
        hasDeathTriggered = false;
        ResetGracePeriod();
        Debug.Log("Réinitialisation après la mort.");
    }
    public void ShowAndHideGainPalier()
    {
        Gain_Palier.SetActive(true); // Active l'objet

        // Apparition
        Gain_Palier.transform.localScale = Vector3.zero; // Commence à scale 0
        Gain_Palier.transform.DOScale(active_scale, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Attend `activeTime` avant de disparaître
            Gain_Palier.transform.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack).SetDelay(activeTime).OnComplete(() =>
            {
                Gain_Palier.SetActive(false); // Désactive l'objet une fois invisible
            });
        });
    }
    public void ShowAndHideLoosePalier()
    {
        Loose_Palier.SetActive(true); // Active l'objet

        // Apparition
        Loose_Palier.transform.localScale = Vector3.zero; // Commence à scale 0
        Loose_Palier.transform.DOScale(active_scale, animationDuration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Attend `activeTime` avant de disparaître
            Loose_Palier.transform.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack).SetDelay(activeTime).OnComplete(() =>
            {
                Loose_Palier.SetActive(false); // Désactive l'objet une fois invisible
            });
        });
    }
}
