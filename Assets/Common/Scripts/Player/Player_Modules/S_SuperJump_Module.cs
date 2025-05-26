using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public float VortexRange;
    }
    [Header("Jump Settings")]
    public List<JumpLevel> jumpLevels; // Liste des niveaux de saut

    [Header("Jump Cooldown")]
    public float jumpCooldown = 1f; // Temps de cooldown entre deux sauts
    
    [Header("Vortex Settings")]
    public LayerMask enemyLayer;
    public float vortexPullSpeed;
    public float vortexDuration;
    public AnimationCurve pullSpeedOverTime;
    
    [Header ("Gizmos Settings")]
    public bool showGizmos = true;
    public float vortexRadius;

    public int _currentJumpCount = 0; // Compteur de sauts utilisés
    private bool _isJumpOnCooldown = false; // Indique si le saut est en cooldown

    // Références aux composants nécessaires
    private S_CustomCharacterController _customCC;
    private S_EnergyStorage _energyStorage;
    private S_InputManager _inputManager;
    private LayerMask _playerBackupLayer;
    private CharacterController _CC;
    
    //Event
    public event Action<Enum,int> OnJumpStateChange;

    private void Start()
    {
        // Initialisation des composants nécessaires
        _customCC = GetComponent<S_CustomCharacterController>();
        _CC = GetComponent<CharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();
        _playerBackupLayer = _CC.excludeLayers;
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

        if (!_customCC.GroundCheck()&&!hadLeaveGround)
        {
            hadLeaveGround = true;
            JumpObserverEvent(PlayerStates.JumpState.OnAir,GetCurrentJumpLevel().level);
        }
        
        // Réinitialiser le compteur de saut si le joueur est au sol
        if (_customCC.GroundCheck()&&hadLeaveGround)
        {
            ResetJumpCount();
            hadLeaveGround = false;
            JumpObserverEvent(PlayerStates.JumpState.OnGround, GetCurrentJumpLevel().level);
        }
        
    }

    private void JumpObserverEvent(Enum JumpState,int level)
    {
        OnJumpStateChange?.Invoke(JumpState,level);
        
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

        // Consommer l'énergie pour le saut
        _energyStorage.RemoveEnergy(currentLevel.energyConsumption);
        
        //Ignore enemy layer
        _CC.excludeLayers = enemyLayer+_playerBackupLayer;
        
        //Active Vortex
        JumpVortex();
        
        //Trigger Evenement
        JumpObserverEvent(PlayerStates.JumpState.Jump,currentLevel.level);
        
        // Appliquer la force de saut
        _customCC.velocity.y = Mathf.Sqrt(currentLevel.jumpHeight * -2f * _customCC.gravity);

        // Incrémenter le compteur de sauts
        _currentJumpCount++;

        // Lancer le cooldown
        StartCoroutine(JumpCooldownRoutine());
    }

    private void JumpVortex()
    {
        StartCoroutine(VortexPullCoroutine());
    }
    private IEnumerator VortexPullCoroutine()
    {
        float elapsed = 0f;
        Vector3 vortexCenter = transform.position;

        List<Rigidbody> affectedBodies = new List<Rigidbody>();
        Collider[] colliders = Physics.OverlapSphere(vortexCenter, GetCurrentJumpLevel().VortexRange, enemyLayer);
        foreach (var col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null&&!col.CompareTag("BossType"))
            {
                affectedBodies.Add(rb);
            }
        }

        while (elapsed < vortexDuration)
        {
            float strength = pullSpeedOverTime.Evaluate(elapsed / vortexDuration);

            foreach (Rigidbody rb in affectedBodies)
            {
                if (rb == null) continue;

                Vector3 dir = (vortexCenter - rb.position).normalized;
                Vector3 move = dir * (vortexPullSpeed * strength * Time.deltaTime);
                rb.MovePosition(rb.position + move);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        _CC.excludeLayers = _playerBackupLayer;
    }
    

    /// <summary>
    /// Obtient le niveau de saut correspondant au niveau d'énergie actuel
    /// </summary>
    /// <returns>Le niveau de saut correspondant ou null si aucun niveau trouvé</returns>
    public JumpLevel GetCurrentJumpLevel()
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

    private void OnDrawGizmos()
    {
        if(!showGizmos)return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, vortexRadius);
    }
}
