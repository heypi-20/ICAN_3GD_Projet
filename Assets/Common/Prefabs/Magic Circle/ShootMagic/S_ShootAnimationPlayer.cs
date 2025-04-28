using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ShootAnimationPlayer : MonoBehaviour
{
    // Reference to the Animator component
    public Animator animator;
    // Name of the Animator state to play "Start Shoot"
    public string StartShootAnimPalier1;
    public string StartShootAnimPalier2;
    public string StartShootAnimPalier3;
    public string StartShootAnimPalier4;
    // Name of the Animator state to play "Stop Shoot"
    public string StopShootAnimPalier1;
    public string StopShootAnimPalier2;
    public string StopShootAnimPalier3;
    public string StopShootAnimPalier4;
    // Name of the Animator state to play "Is Shooting"
    public string IsShootingAnimPalier1;
    public string IsShootingAnimPalier2;
    public string IsShootingAnimPalier3;
    public string IsShootingAnimPalier4;

    public void Start()
    {
        // Warn and exit if still null
        if (animator == null)
        {
            Debug.LogWarning("S_AnimationPlayer: Animator reference not set or found on the GameObject.");
            return;
        }

        S_PlayerStateObserver.Instance.OnShootStateEvent += handleShootAnimation;
    }
    private void handleShootAnimation(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.ShootState.StartShoot when level == 1:
                PlayAnimation(StartShootAnimPalier1);
                break;
            case PlayerStates.ShootState.StartShoot when level == 2:
                PlayAnimation(StartShootAnimPalier2);
                break;
            case PlayerStates.ShootState.StartShoot when level == 3:
                PlayAnimation(StartShootAnimPalier3);
                break;
            case PlayerStates.ShootState.StartShoot when level == 4:
                PlayAnimation(StartShootAnimPalier4);
                break;

            case PlayerStates.ShootState.StopShoot when level == 1:
                PlayAnimation(StopShootAnimPalier1);
                break;
            case PlayerStates.ShootState.StopShoot when level == 2:
                PlayAnimation(StopShootAnimPalier2);
                break;
            case PlayerStates.ShootState.StopShoot when level == 3:
                PlayAnimation(StopShootAnimPalier3);
                break;
            case PlayerStates.ShootState.StopShoot when level == 4:
                PlayAnimation(StopShootAnimPalier4);
                break;

            case PlayerStates.ShootState.IsShooting when level == 1:
                PlayAnimation(IsShootingAnimPalier1);
                break;
            case PlayerStates.ShootState.IsShooting when level == 2:
                PlayAnimation(IsShootingAnimPalier2);
                break;
            case PlayerStates.ShootState.IsShooting when level == 3:
                PlayAnimation(IsShootingAnimPalier3);
                break;
            case PlayerStates.ShootState.IsShooting when level == 4:
                PlayAnimation(IsShootingAnimPalier4);
                break;
        }
    }

    // Play the specified animation state by name
    private void PlayAnimation(string animName)
    {
        // Play the animation state
        animator.Play(animName);
    }
}
