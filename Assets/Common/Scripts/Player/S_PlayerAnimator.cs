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

    }

    private void HandleShootEvent(Enum shootState, int Level)
    {
        switch (shootState)
        {
            case PlayerStates.ShootState.StartShoot:
                animator.CrossFade("StartShoot", 1f);
                break;
            case PlayerStates.ShootState.IsShooting:
                animator.CrossFade("HoldShoot",0.2f);
                break;
            case PlayerStates.ShootState.StopShoot:
                animator.CrossFade("EndShoot",0.2f);
                break;
        }
    }

    private void OnDisable()
    {
        S_PlayerStateObserver.Instance.OnShootStateEvent -= HandleShootEvent;

    }

}
