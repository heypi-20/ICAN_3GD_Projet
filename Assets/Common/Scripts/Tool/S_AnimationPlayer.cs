using System;
using UnityEngine;

public class S_AnimationPlayer : MonoBehaviour
{
    // Reference to the Animator component
    public Animator animator;
    // Name of the Animator state to play
    public string stateName;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PlayAnimation();
        }
    }

    // Play the specified animation state by name
    public void PlayAnimation()
    {
        // If no Animator assigned in Inspector, try to get one from this GameObject
        if (animator == null)
            animator = GetComponent<Animator>();

        // Warn and exit if still null
        if (animator == null)
        {
            Debug.LogWarning("S_AnimationPlayer: Animator reference not set or found on the GameObject.");
            return;
        }

        // Warn and exit if stateName is empty
        if (string.IsNullOrEmpty(stateName))
        {
            Debug.LogWarning("S_AnimationPlayer: stateName is null or empty.");
            return;
        }

        // Play the animation state
        animator.Play(stateName);
    }
}