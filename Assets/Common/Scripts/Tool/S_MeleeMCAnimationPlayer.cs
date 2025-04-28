using System;
using UnityEngine;
using UnityEngine.Serialization;

public class S_MeleeMCAnimationPlayer : MonoBehaviour
{
    // Reference to the Animator component
    public Animator animator;
    // Name of the Animator state to play
    public string StartPunchAnim;
    public string PunchHitAnim;
    public string PunchMissAnim;
    public void Start()
    {
        // Warn and exit if still null
        if (animator == null)
        {
            Debug.LogWarning("S_AnimationPlayer: Animator reference not set or found on the GameObject.");
            return;
        }
        // Warn and exit if stateName is empty
        if (string.IsNullOrEmpty(StartPunchAnim))
        {
            Debug.LogWarning("S_AnimationPlayer: stateName is null or empty.");
            return;
        }
        
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += playMeleeAnimation;
    }

    private void playMeleeAnimation(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.MeleeState.StartMeleeAttack:
                PlayAnimation (StartPunchAnim);
                break;
            case PlayerStates.MeleeState.MeleeAttackHit:
                PlayAnimation (PunchHitAnim);
                break;
            case PlayerStates.MeleeState.MeleeAttackMissed:
                PlayAnimation (PunchMissAnim);
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