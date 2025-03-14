using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;


public class S_MeleeAttack_Module : MonoBehaviour
{
    [System.Serializable]
    public class MeleeAttackLevel
    {
        public int level; // Niveau requis pour ce niveau d'attaque
        public float attackRange; // Rayon de la portée de l'attaque
        public float attackDamage; // Dégâts par attaque
        public int maxTargetsToDestroy; // Nombre maximum de cibles à détruire par attaque
        public float attackCooldown; // Temps de recharge entre les attaques
        public float energyConsumption; // Consommation d'énergie par attaque
        public int dropBonus;
    }

    [Header("Attack Settings")]
    public Transform attackOrigin; // Origine de l'attaque (centre de la sphère)
    public List<MeleeAttackLevel> attackLevels; // Liste des niveaux d'attaque
    public LayerMask targetLayer; // Layer des objets pouvant être détruits

    private S_InputManager _inputManager; // Référence au gestionnaire d'entrées
    private S_EnergyStorage _energyStorage; // Référence au stockage d'énergie
    private float _attackCooldownTimer; // Minuterie pour gérer le temps de recharge
    public float currentAttackCD;

    public event Action<Enum, int> OnAttackStateChange;
    
    private void Start()
    {
        // Initialiser les références
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void Update()
    {
        HandleMeleeAttack();
    }

    private void MeleeAttackObserverEvent(Enum attackState, int currentLevel)
    {
        OnAttackStateChange?.Invoke(attackState, currentLevel);
    }

    private void HandleMeleeAttack()
    {
        MeleeAttackLevel currentLevel = GetCurrentAttackLevel();
        currentAttackCD = currentLevel.attackCooldown;
        
        // Vérifier si le joueur appuie sur la touche d'attaque et si l'attaque est prête
        if (_inputManager.MeleeAttackInput && _attackCooldownTimer <= 0f && _energyStorage.currentEnergy >= currentLevel.energyConsumption&&S_PlayerStateObserver.Instance.LastMeleeState==null)
        {
            SoundManager.Instance.Meth_Active_CAC();
            Debug.Log("logic melee");
            //Trigger start event
            MeleeAttackObserverEvent(PlayerStates.MeleeState.StartMeleeAttack, currentLevel.level);

            PerformMeleeAttack(currentLevel);
            _attackCooldownTimer = currentLevel.attackCooldown; // Réinitialiser la minuterie
            _energyStorage.RemoveEnergy(currentLevel.energyConsumption); // Consommer de l'énergie
            StartCoroutine(StartTimer(_attackCooldownTimer));
        }

        // Réduire la minuterie de recharge
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }
        
    }

    private IEnumerator StartTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        MeleeAttackObserverEvent(PlayerStates.MeleeState.EndMeleeAttack, GetCurrentAttackLevel().level);
        _inputManager.MeleeAttackInput = false;

    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void PerformMeleeAttack(MeleeAttackLevel currentLevel)
    {
        // Détecter toutes les cibles dans la portée de l'attaque
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position + attackOrigin.forward * currentLevel.attackRange, currentLevel.attackRange, targetLayer);

        if (hits.Length > 0)
        {
            MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHit,GetCurrentAttackLevel().level);
        }
        else if(hits.Length == 0)
        {
            MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackMissed,GetCurrentAttackLevel().level);
        }
        
        // Trier les cibles par distance
        List<Collider> sortedTargets = new List<Collider>(hits);
        sortedTargets.Sort((a, b) =>
            Vector3.Distance(attackOrigin.position, a.transform.position)
                .CompareTo(Vector3.Distance(attackOrigin.position, b.transform.position)));

        // Détruire jusqu'à un nombre maximum de cibles
        int targetsDestroyed = 0;
        foreach (Collider target in sortedTargets)
        {
            if (targetsDestroyed >= currentLevel.maxTargetsToDestroy)
                break;

            Rigidbody targetRB = target.gameObject.GetComponent<Rigidbody>();
            
            if (targetRB!=null)
            {
                Vector3 forceDirection = targetRB.transform.position - transform.position;
                forceDirection.Normalize();
                targetRB.AddForce(forceDirection*20f, ForceMode.Impulse);
            }
            target.gameObject.GetComponent<EnemyBase>()?.ReduceHealth(GetCurrentAttackLevel().attackDamage, GetCurrentAttackLevel().dropBonus);
            targetsDestroyed++;
        }
        
        
    }

    private MeleeAttackLevel GetCurrentAttackLevel()
    {
        // Vérifier si _energyStorage est initialisé
        if (_energyStorage == null)
        {
            _energyStorage = GetComponent<S_EnergyStorage>();
        }

        if (_energyStorage == null) return null;

        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Ajustement pour correspondre aux niveaux
        return attackLevels.Find(level => level.level == currentLevelIndex);
    }

    private void OnDrawGizmos()
    {
        if (attackOrigin != null && attackLevels != null && attackLevels.Count > 0)
        {
            MeleeAttackLevel currentLevel = GetCurrentAttackLevel();
            if (currentLevel != null)
            {
                // Dessiner une sphère pour représenter la portée de l'attaque
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * currentLevel.attackRange, currentLevel.attackRange);
            }
        }
    }
}
