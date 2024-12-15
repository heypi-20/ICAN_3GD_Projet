using System;
using Unity.VisualScripting;
using UnityEngine;

public class S_MeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.E;
    public Transform attackPoint;
    public bool showGizmos = true;

    [Header("Attack Properties")]
    [Range(0.1f, 1.5f)]
    public float range = 1f;
    public float attackCD = 0.2f;
    
    private S_PlayerMultiCam p;
    private RaycastHit attackHit;

    private bool canAttack;
    private float timer;

    private void Start()
    {
        p = GetComponent<S_PlayerMultiCam>();
    }

    private void Update()
    {
        AttackCooldown();
        
        if (Input.GetKeyDown(attackKey) && canAttack) {
            Attack();
        }
    }

    private void Attack()
    {
        if (Physics.SphereCast(attackPoint.position, GetComponent<CapsuleCollider>().height / 2, Camera.main.transform.forward * range, out attackHit, range)) {
            Debug.Log("attackHit");
        }
        
        canAttack = false;
    }

    private void AttackCooldown()
    {
        timer += Time.deltaTime;
        
        if (timer >= attackCD) {
            timer = 0;
            canAttack = true;
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos) {
            Gizmos.DrawWireSphere(attackPoint.position, range);
        
            if (Physics.SphereCast(attackPoint.position, GetComponent<CapsuleCollider>().height / 2, Camera.main.transform.forward * range, out attackHit, range)) {
                Gizmos.color = Color.green;
                Vector3 sphereCastMidpoint = attackPoint.position + (Camera.main.transform.forward * attackHit.distance);
                Gizmos.DrawWireSphere(sphereCastMidpoint, GetComponent<CapsuleCollider>().height / 2);
                Gizmos.DrawSphere(attackHit.point, 0.1f);
                Debug.DrawLine(attackPoint.position, sphereCastMidpoint, Color.green);
            }
            else
            {
                Gizmos.color = Color.red;
                Vector3 sphereCastMidpoint = attackPoint.position + (Camera.main.transform.forward * (range - (GetComponent<CapsuleCollider>().height / 2)));
                Gizmos.DrawWireSphere(sphereCastMidpoint, GetComponent<CapsuleCollider>().height / 2);
                Debug.DrawLine(attackPoint.position, sphereCastMidpoint, Color.red);
            }
        }
    }
}
