using System.Collections.Generic;
using UnityEngine;

public class S_MeleeAttack_Module : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackOrigin; // Origine de l'attaque (centre de la sphère)
    public float attackRange = 5f; // Rayon de la portée de l'attaque
    public int maxTargetsToDestroy = 5; // Nombre maximum de cibles à détruire par attaque
    public LayerMask targetLayer; // Layer des objets pouvant être détruits
    public float attackCooldown = 2f; // Temps de recharge entre les attaques

    private S_InputManager _inputManager; // Référence au gestionnaire d'entrées
    private float _attackCooldownTimer; // Minuterie pour gérer le temps de recharge

    private void Start()
    {
        // Initialiser la référence au gestionnaire d'entrées
        _inputManager = FindObjectOfType<S_InputManager>();
    }

    private void Update()
    {
        HandleMeleeAttack();
    }

    private void HandleMeleeAttack()
    {
        // Vérifier si le joueur appuie sur la touche d'attaque et si l'attaque est prête
        if (_inputManager.MeleeAttackInput && _attackCooldownTimer <= 0f)
        {
            PerformMeleeAttack();
            _attackCooldownTimer = attackCooldown; // Réinitialiser la minuterie
        }

        // Réduire la minuterie de recharge
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void PerformMeleeAttack()
    {
        // Détecter toutes les cibles dans la portée de l'attaque
        Collider[] hits = Physics.OverlapSphere(attackOrigin.position + attackOrigin.forward * attackRange, attackRange, targetLayer);

        // Trier les cibles par distance
        List<Collider> sortedTargets = new List<Collider>(hits);
        sortedTargets.Sort((a, b) =>
            Vector3.Distance(attackOrigin.position, a.transform.position)
                .CompareTo(Vector3.Distance(attackOrigin.position, b.transform.position)));

        // Détruire jusqu'à un nombre maximum de cibles
        int targetsDestroyed = 0;
        foreach (Collider target in sortedTargets)
        {
            if (targetsDestroyed >= maxTargetsToDestroy)
                break;

            target.gameObject.GetComponent<S_DroppingModule>().DropItems(7f);
            target.gameObject.GetComponent<S_DestructionModule>().DestroyObject();
            targetsDestroyed++;
        }
    }

    private void OnDrawGizmos()
    {
        if (attackOrigin != null)
        {
            // Dessiner une sphère pour représenter la portée de l'attaque
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position + attackOrigin.forward * attackRange, attackRange);
        }
    }
}
