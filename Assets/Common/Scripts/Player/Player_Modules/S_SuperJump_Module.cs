using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_CustomCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_SuperJump_Module : MonoBehaviour
{
    
    [System.Serializable]
    public class JumpLevel
    {
        public int level; // Niveau d'énergie requis pour ce saut
        public float jumpHeight; // Hauteur du saut pour ce niveau
        public float energyConsumption; // Consommation d'énergie pour ce niveau
        public int maxJumpCount; // Nombre maximum de sauts permis pour ce niveau
    }
    [Header("Jump Settings")]
    public List<JumpLevel> jumpLevels; // Liste des niveaux de saut

    [Header("Jump Cooldown")]
    public float jumpCooldown = 1f; // Temps de cooldown entre deux sauts

    private int _currentJumpCount = 0; // Compteur de sauts utilisés
    private bool _isJumpOnCooldown = false; // Indique si le saut est en cooldown

    // Références aux composants nécessaires
    private S_CustomCharacterController _characterController;
    private S_EnergyStorage _energyStorage;
    private S_InputManager _inputManager;
    
    //Event
    public event Action<Enum> OnJumpStateChange;

    private void Start()
    {
        // Initialisation des composants nécessaires
        _characterController = GetComponent<S_CustomCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();
    }

    private bool hadLeaveGround;
    private void Update()
    {
        // Vérifier si le joueur appuie sur la touche de saut
        if (_inputManager.JumpInput)
        {
            AttemptJump(); // Tenter de sauter
            _inputManager.JumpInput = false; // Réinitialiser l'input pour éviter plusieurs sauts dans la même frame
        }

        if (!_characterController.GroundCheck()&&!hadLeaveGround)
        {
            hadLeaveGround = true;
            JumpObserverEvent(PlayerStates.JumpState.OnAir);
        }
        
        // Réinitialiser le compteur de saut si le joueur est au sol
        if (_characterController.GroundCheck()&&hadLeaveGround)
        {
            ResetJumpCount();
            hadLeaveGround = false;
            JumpObserverEvent(PlayerStates.JumpState.OnGround);
        }
        
    }

    private void JumpObserverEvent(Enum JumpState)
    {
        OnJumpStateChange?.Invoke(JumpState);
        
    }
    
    

    /// <summary>
    /// Tente un saut en fonction de l'état actuel (au sol, énergie disponible, etc.)
    /// </summary>
    private void AttemptJump()
    {
        // Si en cooldown, ne pas sauter
        if (_isJumpOnCooldown) return;

        // Vérifier si le joueur peut encore sauter (nombre de sauts restants)
        JumpLevel currentLevel = GetCurrentJumpLevel();
        if (currentLevel == null || _currentJumpCount >= currentLevel.maxJumpCount) return;

        // Vérifier si l'énergie est suffisante pour sauter
        if (_energyStorage.currentEnergy < currentLevel.energyConsumption)
        {
            return;
        }

        // Consommer l'énergie pour le saut
        _energyStorage.RemoveEnergy(currentLevel.energyConsumption);
        
        //Audio OnJump
        SoundManager.Instance.Meth_Used_Jump();
        
        //Trigger Evenement
        JumpObserverEvent(PlayerStates.JumpState.Jump);
        
        // Appliquer la force de saut
        _characterController.velocity.y = Mathf.Sqrt(currentLevel.jumpHeight * -2f * _characterController.gravity);

        // Incrémenter le compteur de sauts
        _currentJumpCount++;

        // Lancer le cooldown
        StartCoroutine(JumpCooldownRoutine());

        // TODO : Ajouter un feedback visuel ou sonore ici
    }

    /// <summary>
    /// Obtient le niveau de saut correspondant au niveau d'énergie actuel
    /// </summary>
    /// <returns>Le niveau de saut correspondant ou null si aucun niveau trouvé</returns>
    private JumpLevel GetCurrentJumpLevel()
    {
        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Ajuster pour correspondre aux niveaux (index 0 -> niveau 1)
        return jumpLevels.Find(level => level.level == currentLevelIndex);
    }

    /// <summary>
    /// Réinitialise le compteur de sauts lorsque le joueur touche le sol.
    /// </summary>
    private void ResetJumpCount()
    {
        _currentJumpCount = 0;
    }

    /// <summary>
    /// Coroutine gérant le cooldown après un saut
    /// </summary>
    /// <returns>Une IEnumerator pour le cooldown</returns>
    private IEnumerator JumpCooldownRoutine()
    {
        _isJumpOnCooldown = true;
        yield return new WaitForSeconds(jumpCooldown);
        _isJumpOnCooldown = false;
    }
}
