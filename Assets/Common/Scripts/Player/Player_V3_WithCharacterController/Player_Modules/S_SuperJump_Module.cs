using System.Collections;
using UnityEngine;
using UnityEditor;

public class S_SuperJump_Module : MonoBehaviour
{
    [Header("Jump Force Settings")]
    public float minJumpForce = 3f;
    public float maxJumpForce = 5f;
    public float jumpForcePercentage;
    public float jumpForceMultiplier;
    
    [Header("Energy Consumption Settings")]
    public float baseEnergyConsumption = 35f;      
    public float extraConsumptionMultiplier = 2f;

    [Header("Jump Cooldown Settings")]
    public float jumpCooldown = 1f;
    
    private float _currentJumpForce;
    private float _currentEnergyConsumption; 
    private bool _isJumpOnCooldown = false;
    
    // Composant pour ajuster la vitesse de déplacement
    private S_CustomCharacterController _characterController;
    // Composant pour vérifier et modifier l'énergie disponible
    private S_EnergyStorage _energyStorage;
    // Gestionnaire d'input pour détecter les actions du joueur
    private S_InputManager _inputManager;

    // ----------------------------------
    // Seulement pour l'affichage dans l'Inspector
    // ----------------------------------
    public float estimatedMaxJumpEnergyThreshold { get; private set; }

    private void Start()
    {
        // Initialisation des composants
        _characterController = GetComponent<S_CustomCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();
        
        // Initialisation de la consommation d'énergie à la valeur de base
        _currentEnergyConsumption = baseEnergyConsumption;
    }

    private void Update()
    {
        // Vérifier si le joueur appuie sur la touche de saut
        if (_inputManager.JumpInput)
        {
            // Tenter l'action de saut
            AttemptJump();
            // Réinitialiser l'input pour éviter plusieurs sauts dans la même frame
            _inputManager.JumpInput = false;
        }
    }
    /// <summary>
    /// Tenter un saut, comprenant le check de cooldown et d'énergie
    /// </summary>
    private void AttemptJump()
    {
        // Si on est en cooldown, on ne saute pas
        if (_isJumpOnCooldown) return;

        // Vérifier si le personnage est au sol
        bool isOnGround = _characterController.GroundCheck();
        if (isOnGround)
        {
            // Réinitialiser la consommation d'énergie si le joueur est au sol
            _currentEnergyConsumption = baseEnergyConsumption;
        }
        else
        {
            // Si le joueur est en l'air, multiplier la consommation d'énergie
            _currentEnergyConsumption *= extraConsumptionMultiplier;
        }
        
        
        // // Vérifier si l'énergie est suffisante
        // if (_energyStorage.currentEnergy >= _currentEnergyConsumption)
        // {
        //
        // }
        // Retirer l'énergie
        _energyStorage.RemoveEnergy(_currentEnergyConsumption); 
        
        //TODO : add FeedBack
        
        //
        // Calculer la force de saut
        CalculateBonusJumpForce();

        // Appliquer la vélocité de saut
        _characterController.velocity.y = Mathf.Sqrt(_currentJumpForce * -2f * _characterController.gravity);

        // Lancer le cooldown
        StartCoroutine(JumpCooldownRoutine());
        

    }

    /// <summary>
    /// Calculer la force de saut supplémentaire liée à l'énergie, 
    /// en la limitant entre minJumpForce et maxJumpForce.
    /// </summary>
    private void CalculateBonusJumpForce()
    {
        _currentJumpForce = (_energyStorage.currentEnergy * jumpForcePercentage) * jumpForceMultiplier;
        _currentJumpForce = Mathf.Clamp(_currentJumpForce, minJumpForce, maxJumpForce);
    }

    /// <summary>
    /// Coroutine de cooldown de saut, attend un certain délai avant de réautoriser le saut.
    /// </summary>
    private IEnumerator JumpCooldownRoutine()
    {
        _isJumpOnCooldown = true;
        yield return new WaitForSeconds(jumpCooldown);
        _isJumpOnCooldown = false;
    }

    // --------------------------------------------------------------------------------
    // Calcule la quantité d'énergie approximative nécessaire pour atteindre maxJumpForce
    // --------------------------------------------------------------------------------
    private void OnValidate()
    {
        EstimateEnergyThresholdForMaxJump(out float threshold);
        estimatedMaxJumpEnergyThreshold = threshold;
    }

    private void EstimateEnergyThresholdForMaxJump(out float threshold)
    {
        if (jumpForcePercentage <= 0f || jumpForceMultiplier <= 0f)
        {
            threshold = 0f;
        }
        else
        {
            threshold = maxJumpForce / (jumpForcePercentage * jumpForceMultiplier);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(S_SuperJump_Module))]
public class S_SuperJump_ModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        S_SuperJump_Module module = (S_SuperJump_Module)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Estimated Jump Thresholds", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Max Jump Energy Threshold:", module.estimatedMaxJumpEnergyThreshold.ToString("F2"));
    }
}
#endif
