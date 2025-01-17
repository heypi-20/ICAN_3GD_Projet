using System;
using UnityEngine;
using DG.Tweening;

public class S_OldMeleeAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.E;
    public Transform attackPoint;
    public GameObject attackObject;

    [Header("Attack Properties")]
    public float attackCD = 0.5f;
    public float attackDuration = 0.3f;
    public float swingAngle = 180f; // Default to full swing for clarity

    private bool canAttack = true;
    private bool isAttacking = false;
    private float timer = 0f;
    private Tween attackTween;

    private void Update()
    {
        AttackCooldown();

        if (Input.GetKeyDown(attackKey) && canAttack)
        {
            Attack();
        }

        // Ensure the attack object always follows the attack point
        if (isAttacking && attackObject.activeSelf)
        {
            attackObject.transform.position = attackPoint.position;
        }
    }

    private void Attack()
    {
        if (isAttacking)
        {
            attackTween?.Kill(); // Interrupt current animation
        }

        canAttack = false;
        isAttacking = true;

        // Activate attack object
        attackObject.SetActive(true);
        attackObject.transform.position = attackPoint.position;

        // Perform swing animation relative to attackPoint's orientation
        Quaternion startRotation = attackPoint.rotation * Quaternion.Euler(0, -swingAngle / 2, 0);
        Quaternion endRotation = attackPoint.rotation * Quaternion.Euler(0, swingAngle / 2, 0);

        attackObject.transform.rotation = startRotation;
        attackTween = attackObject.transform.DORotateQuaternion(endRotation, attackDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                isAttacking = false;
                attackObject.SetActive(false); // Deactivate after animation
            });
    }

    private void AttackCooldown()
    {
        if (!canAttack)
        {
            timer += Time.deltaTime;
            if (timer >= attackCD)
            {
                timer = 0;
                canAttack = true;
            }
        }
    }
}
