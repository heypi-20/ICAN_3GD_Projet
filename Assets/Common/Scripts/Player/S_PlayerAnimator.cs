using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        S_PlayerStateObserver.Instance.OnShootStateEvent += HandleCombatEvent;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += HandleCombatEvent;

    }

    private void HandleCombatEvent(Enum CombatState, int Level)
    {
        switch (CombatState)
        {
            case PlayerStates.ShootState.StartShoot when S_PlayerStateObserver.Instance.LastMeleeState==null:
                animator.CrossFade("StartShoot", 1f);
                break;
            case PlayerStates.ShootState.IsShooting when S_PlayerStateObserver.Instance.LastMeleeState==null:
                animator.CrossFade("HoldShoot",0.2f);
                break;
            case PlayerStates.ShootState.StopShoot:
                animator.CrossFade("Idle",1f);
                break;
            case PlayerStates.MeleeState.StartMeleeAttack:
                animator.CrossFade("Punch", 0.2f);
                break;
            case PlayerStates.MeleeState.EndMeleeAttack:
                animator.CrossFade("Idle",0.2f);
                break;

        }
    }

    

    private void OnDisable()
    {
        S_PlayerStateObserver.Instance.OnShootStateEvent -= HandleCombatEvent;

    }

}
