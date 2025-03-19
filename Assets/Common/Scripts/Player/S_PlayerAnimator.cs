using System;
using DG.Tweening;
using UnityEngine;

public class S_PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform; // The transform of the FPS character's hand/weapon
    
    [Header("Jump Animation Settings")]
    public float jumpOffsetY = -0.2f; // Downward offset when jumping
    public float jumpOffsetZ = 0.1f;  // Forward offset when jumping
    public float jumpDropTime = 0.05f; // Time for hand to drop quickly
    public float jumpReturnTime = 0.3f; // Time for hand to slowly return to normal

    [Header("Landing Animation Settings")]
    public float landOffsetY = 0.1f; // Upward bounce when landing
    public float landBounceTime = 0.05f; // Time for quick upward motion
    public float landReturnTime = 0.15f; // Time to smoothly return to normal
    private Vector3 defaultHandPosition;
    
    private Animator animator;
    private int skillLayerIndex;

    private void Start()
    {
        animator = GetComponent<Animator>();
        skillLayerIndex = animator.GetLayerIndex("SkillLayer");
        
        //Store the default hand position
        defaultHandPosition = handTransform.localPosition;
        
        
        S_PlayerStateObserver.Instance.OnShootStateEvent += HandleShootEvent;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += HandleMeleeState;
        S_PlayerStateObserver.Instance.OnJumpStateEvent+= HandleJumpState;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += HandleGroundPoundState;
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
            case PlayerStates.MeleeState.StartMeleeAttack when S_PlayerStateObserver.Instance.LastGroundPoundState==null:
                animator.CrossFade("Punch", 0.2f);
                break;
            case PlayerStates.MeleeState.EndMeleeAttack when S_PlayerStateObserver.Instance.LastGroundPoundState==null:
                animator.CrossFade("Idle",0.2f);
                break;
        }
    }

    private void HandleJumpState(Enum JumpState)
    {
        switch (JumpState)
        {
            case PlayerStates.JumpState.Jump:
                animator.SetLayerWeight(skillLayerIndex, 1f); 
                animator.Play("UseRune", skillLayerIndex, 0f); 
                JumpHandAnimation();

                break;
            case PlayerStates.JumpState.OnGround:
                animator.SetLayerWeight(skillLayerIndex, 0f); 
                ResetHandPosition();
                break;
        }
    }

    private void HandleGroundPoundState(Enum GroundPoundState)
    {
        switch (GroundPoundState)
        {
            case PlayerStates.GroundPoundState.StartGroundPound:
                animator.CrossFade("StartGroundPound",0.2f);
                break;
            case PlayerStates.GroundPoundState.isGroundPounding:
                animator.CrossFade("HoldGroundPound",0.2f);
                break;
            case PlayerStates.GroundPoundState.EndGroundPound:
                animator.CrossFade("EndGroundPound",0.2f);
                break;
        }
    }
    
    private void JumpHandAnimation()
    {
        // Kill any existing DOTween animations on handTransform to prevent conflicts
        handTransform.DOKill();

        // Move the hand quickly downward and slightly forward
        handTransform.DOLocalMove(defaultHandPosition + new Vector3(0, jumpOffsetY, jumpOffsetZ), jumpDropTime)
            .SetEase(Ease.InQuad) // Fast drop
            .OnComplete(() =>
            {
                // Slowly move the hand back to default position
                handTransform.DOLocalMove(defaultHandPosition, jumpReturnTime).SetEase(Ease.OutQuad);
            });
    }

    private void ResetHandPosition()
    {
        // Kill any existing DOTween animations on handTransform to prevent conflicts
        handTransform.DOKill();

        // Move the hand slightly upward quickly (impact effect)
        handTransform.DOLocalMove(defaultHandPosition + new Vector3(0, landOffsetY, 0), landBounceTime)
            .SetEase(Ease.OutQuad) // Quick upward reaction
            .OnComplete(() =>
            {
                // Then smoothly move back to default position
                handTransform.DOLocalMove(defaultHandPosition, landReturnTime).SetEase(Ease.InOutQuad);
            });
    }
    

    

    private void OnDisable()
    {
        S_PlayerStateObserver.Instance.OnShootStateEvent -= HandleShootEvent;

    }

}
