using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ShootAnimationPlayer : MonoBehaviour
{
    // Reference to the Animator component
    public Animator animator;
    // Name of the Animator state to play
    public string StartShootAnim;
    
    public void Start()
    {
        // Warn and exit if still null
        if (animator == null)
        {
            Debug.LogWarning("S_AnimationPlayer: Animator reference not set or found on the GameObject.");
            return;
        }
        // Warn and exit if stateName is empty
        if (string.IsNullOrEmpty(StartShootAnim))
        {
            Debug.LogWarning("S_AnimationPlayer: stateName is null or empty.");
            return;
        }

        S_PlayerStateObserver.Instance.OnShootStateEvent += handleShootAnimation;
    }

    private void handleShootAnimation(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.ShootState.StartShoot:
                PlayAnimation(StartShootAnim);
                break;
            //case PlayerStates.MeleeState.MeleeAttackHit:
            //    PlayAnimation(PunchHitAnim);
            //    break;
            //case PlayerStates.MeleeState.MeleeAttackMissed:
            //    PlayAnimation(PunchMissAnim);
            //    break;
        }
    }

    // Play the specified animation state by name
    private void PlayAnimation(string animName)
    {
        // Play the animation state
        animator.Play(animName);
    }
}
