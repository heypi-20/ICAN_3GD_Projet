using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class S_GroundPound_Module : MonoBehaviour
{
    [System.Serializable]
    public class GroundPoundLevel
    {
        public int level; // Niveau requis
        public float sphereRange; // Portée de la détection sphérique
        public float sphereDamage; // Dégâts sphériques (non utilisé pour l'instant)
        public float descentSpeed; // Vitesse de descente
        public float energyConsumption; // Consommation d'énergie
    }

    [Header("Paramètres du Ground Pound")]
    public List<GroundPoundLevel> groundPoundLevels; // Paramètres pour différents niveaux
    public LayerMask targetLayer; // Couches cibles pour la détection sphérique
    public float angleThreshold = 75f; // Seuil d'angle pour vérifier si la caméra regarde vers le bas (0 = totalement vers le bas)
    public float minimumGroundDistance; // Distance minimale au sol

    private S_InputManager _inputManager; // Gestionnaire des entrées utilisateur
    private S_EnergyStorage _energyStorage; // Stockage d'énergie
    private Transform _cameraTransform; // Transform de la caméra (extrait de CinemachineBrain)
    private CharacterController _characterController; // Contrôleur de personnage
    private S_CustomCharacterController _customCharacterController;
    private bool _isGrounded = false; // Vérifie si le joueur a touché le sol
    private bool _isGroundPounding = false; // Indique si la compétence est en cours

    private void Start()
    {
        // Initialiser les références
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _characterController = GetComponent<CharacterController>();
        _customCharacterController = GetComponent<S_CustomCharacterController>();
        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleGroundPound();
    }

    private void HandleGroundPound()
    {
        if (_isGroundPounding) return; // Ignorer si la compétence est déjà en cours

        GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
        if (currentLevel == null) return;

        // Vérifier si le joueur appuie sur la touche d'attaque et si l'énergie est suffisante
        if (_inputManager.MeleeAttackInput && _energyStorage.currentEnergy >= currentLevel.energyConsumption)
        {
            if (IsLookingDownward() && IsGroundFarEnough())
            {
                PerformGroundPound(currentLevel);
                _energyStorage.RemoveEnergy(currentLevel.energyConsumption); // Consommer de l'énergie une seule fois
            }
        }
    }

    private bool IsLookingDownward()
    {
        if (_cameraTransform == null) return false;

        float angle = Vector3.Angle(_cameraTransform.forward, Vector3.down);
        return angle <= angleThreshold;
    }

    private bool IsGroundFarEnough()
    {
        

        if (Physics.Raycast(transform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity))
        {
            float distance = hit.distance;
            if (distance > minimumGroundDistance) // Vérifie si la distance au sol dépasse le minimum
            {
                return true;
            }
        }
        return false;
    }

    private void PerformGroundPound(GroundPoundLevel currentLevel)
    {
        _isGroundPounding = true; // Marquer la compétence comme active
        StopAllCoroutines(); // Arrête les coroutines précédentes (si nécessaire)
        Vector3 direction = (_cameraTransform.forward).normalized;
        StartCoroutine(MoveToGround(direction, currentLevel.descentSpeed));
        Debug.Log("trigger");
    }

    private IEnumerator MoveToGround(Vector3 poundDirection, float speed)
    {
        
        while (!_isGrounded)
        {
            if (_customCharacterController.GroundCheck())
            {
                _isGrounded = true;
                yield return null;// Confirme que le joueur a touché le sol
            }
            
            _characterController.Move(poundDirection * (speed * Time.deltaTime));
            yield return null;
        }
        if (_isGrounded)
        {
            _characterController.Move(Vector3.zero);
            TriggerGroundPoundEffect(); // Exécute l'effet une fois au sol
        }
        _isGroundPounding = false; // Réinitialiser l'état de la compétence
    }

    private void TriggerGroundPoundEffect()
    {
        GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
        if (currentLevel == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, currentLevel.sphereRange, targetLayer);

        foreach (Collider hit in hits)
        {
            hit.GetComponent<S_DestructionModule>()?.DestroyObject();
            hit.GetComponent<S_DroppingModule>()?.DropItems(5f);
        }

        _isGrounded = false; // Réinitialise l'état au sol pour le prochain Ground Pound
    }

    private GroundPoundLevel GetCurrentGroundPoundLevel()
    {
        if (_energyStorage == null) return null;

        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Supposons que les niveaux commencent à 1
        return groundPoundLevels.Find(level => level.level == currentLevelIndex);
    }

    private void OnDrawGizmos()
    {
        if (groundPoundLevels != null && groundPoundLevels.Count > 0)
        {
            GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
            if (currentLevel != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, currentLevel.sphereRange);
            }
        }

        // Afficher la distance minimale au sol en tant que ligne
        Gizmos.color = Color.green;
        Vector3 startPoint = transform.position;
        Vector3 endPoint = transform.position + Vector3.down * minimumGroundDistance;
        Gizmos.DrawLine(startPoint, endPoint);

        // Marquer le point final avec une petite sphère
        Gizmos.color = Color.red;   
        Gizmos.DrawSphere(endPoint, 0.1f);
    }
}
