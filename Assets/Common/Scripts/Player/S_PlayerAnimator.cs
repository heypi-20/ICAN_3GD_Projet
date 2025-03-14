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
        S_PlayerStateObserver.Instance.OnShootStateEvent += HandleShootEvent;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += HandleMeleeState;

    }

    private void HandleShootEvent(Enum ShootState, int Level)
    {
        switch (ShootState)
        {
            case PlayerStates.ShootState.StartShoot when S_PlayerStateObserver.Instance.LastMeleeState==null:
                animator.CrossFade("StartShoot", 0.2f);
                break;
            case PlayerStates.ShootState.IsShooting when S_PlayerStateObserver.Instance.LastMeleeState==null:
                animator.CrossFade("HoldShoot",0.2f);
                break;
            case PlayerStates.ShootState.StopShoot when S_PlayerStateObserver.Instance.LastMeleeState==null:
                animator.CrossFade("EndShoot",0.2f);
                break;

        }
    }

    private void HandleMeleeState(Enum MeleeState, int Level)
    {
        switch (MeleeState)
        {
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
        S_PlayerStateObserver.Instance.OnShootStateEvent -= HandleShootEvent;

    }

}
