using System;
using DG.Tweening;
using Unity.VisualScripting;
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

    [Header("Movement Swing Settings")]
    public Vector3 swingAmplitude = new Vector3(0.05f, 0.05f, 0f); // Amplitude of hand swing when moving
    public float swingDuration = 0.5f; // Duration for half swing cycle
    public Ease swingEase = Ease.InOutSine; // Ease type for natural acceleration and deceleration

    private Vector3 defaultHandPosition; // The default local position of the hand
    private Tween moveSwingTween; // Tween for movement hand swing animation
    private Tween jumpTween;      // Tween for jump hand animation

    private Animator animator;
    private int skillLayerIndex;

    // Reference to the custom character controller for ground checking
    private S_CustomCharacterController characterController;

    // Boolean to track if the player is currently moving
    private bool isMoving = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        skillLayerIndex = animator.GetLayerIndex("SkillLayer");
        characterController = GetComponent<S_CustomCharacterController>(); // Get the custom character controller

        // Store the default hand position
        defaultHandPosition = handTransform.localPosition;

        S_PlayerStateObserver.Instance.OnShootStateEvent += HandleShootEvent;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += HandleMeleeState;
        S_PlayerStateObserver.Instance.OnJumpStateEvent += HandleJumpState;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += HandleGroundPoundState;
        S_PlayerStateObserver.Instance.OnMoveStateEvent += HandleMoveEvent;
    }

    private void Update()
    {
        // When the player is moving, check if they're grounded.
        // If not, pause the hand swing tween; if yes and the tween is paused, resume it.
        if (isMoving && characterController != null)
        {
            bool grounded = characterController.GroundCheck();
            if (!grounded)
            {
                if (moveSwingTween != null && moveSwingTween.IsPlaying())
                {
                    moveSwingTween.Pause();
                }
            }
            else
            {
                if (moveSwingTween != null && !moveSwingTween.IsPlaying())
                {
                    moveSwingTween.Play();
                }
            }
        }
    }

    // Handle movement events to start and stop hand swing animation
    private void HandleMoveEvent(Enum moveState, Vector2 direction)
    {
        switch (moveState)
        {
            case PlayerStates.MoveState.StartMoving:
                isMoving = true;
                StartHandSwing();
                break;
            case PlayerStates.MoveState.StopMoving:
                isMoving = false;
                StopHandSwing();
                break;
        }
    }

    // Start hand swing animation using DOTween's Yoyo loop
    private void StartHandSwing()
    {
        // Only start the hand swing if the player is grounded
        if (characterController != null && !characterController.GroundCheck())
        {
            return;
        }
        // Kill existing move swing tween if active to avoid duplicates
        if (moveSwingTween != null && moveSwingTween.IsActive())
        {
            moveSwingTween.Kill();
        }
        // Create a looping tween that moves the hand to an offset position and back to default
        moveSwingTween = handTransform.DOLocalMove(defaultHandPosition + swingAmplitude, swingDuration)
            .SetEase(swingEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId("MoveSwingTween");
    }

    // Stop hand swing animation and smoothly return the hand to its default position
    private void StopHandSwing()
    {
        if (moveSwingTween != null)
        {
            moveSwingTween.Kill();
            moveSwingTween = null;
        }
        // Only run the return tween if the player is grounded
        if (characterController != null && !characterController.GroundCheck())
        {
            return;
        }
        handTransform.DOLocalMove(defaultHandPosition, 0.2f).SetEase(Ease.OutQuad);
    }

    private void HandleShootEvent(Enum ShootState, int Level)
    {
        switch (ShootState)
        {
            case PlayerStates.ShootState.StartShoot when S_PlayerStateObserver.Instance.LastMeleeState == null:
                animator.CrossFade("StartShoot", 0.2f);
                break;
            case PlayerStates.ShootState.IsShooting when S_PlayerStateObserver.Instance.LastMeleeState == null:
                animator.CrossFade("HoldShoot", 0.2f);
                break;
            case PlayerStates.ShootState.StopShoot when S_PlayerStateObserver.Instance.LastMeleeState == null:
                animator.CrossFade("EndShoot", 0.2f);
                break;
        }
    }

    private void HandleMeleeState(Enum MeleeState, int Level)
    {
        switch (MeleeState)
        {
            case PlayerStates.MeleeState.StartMeleeAttack when S_PlayerStateObserver.Instance.LastGroundPoundState == null:
                animator.CrossFade("Punch", 0.2f);
                break;
            case PlayerStates.MeleeState.EndMeleeAttack when S_PlayerStateObserver.Instance.LastGroundPoundState == null:
                animator.CrossFade("Idle", 0.2f);
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
                ResetHandPositionWhenLanding();
                break;
        }
    }

    private void HandleGroundPoundState(Enum GroundPoundState)
    {
        switch (GroundPoundState)
        {
            case PlayerStates.GroundPoundState.StartGroundPound:
                animator.CrossFade("StartGroundPound", 0.2f);
                break;
            case PlayerStates.GroundPoundState.isGroundPounding:
                animator.CrossFade("HoldGroundPound", 0.2f);
                break;
            case PlayerStates.GroundPoundState.EndGroundPound:
                animator.CrossFade("EndGroundPound", 0.2f);
                break;
        }
    }

    // Hand animation for jump event
    private void JumpHandAnimation()
    {
        // Pause the move swing tween if active to avoid conflicts
        if (moveSwingTween != null && moveSwingTween.IsActive())
        {
            moveSwingTween.Pause();
        }
        // Create a jump sequence tween
        jumpTween = DOTween.Sequence()
            .Append(handTransform.DOLocalMove(defaultHandPosition + new Vector3(0, jumpOffsetY, jumpOffsetZ), jumpDropTime)
                .SetEase(Ease.InQuad)
                .SetId("JumpTween"))
            .Append(handTransform.DOLocalMove(defaultHandPosition, jumpReturnTime)
                .SetEase(Ease.OutQuad)
                .SetId("JumpTween"))
            .OnComplete(() =>
            {
                // Resume the move swing tween after jump if the player is grounded
                if (characterController != null && characterController.GroundCheck() && moveSwingTween != null)
                {
                    moveSwingTween.Play();
                }
            });
    }

    // Hand animation for landing event
    private void ResetHandPositionWhenLanding()
    {
        // Pause the move swing tween if active to avoid conflicts
        if (moveSwingTween != null && moveSwingTween.IsActive())
        {
            moveSwingTween.Pause();
        }
        Tween landTween = DOTween.Sequence()
            .Append(handTransform.DOLocalMove(defaultHandPosition + new Vector3(0, landOffsetY, 0), landBounceTime)
                .SetEase(Ease.OutQuad)
                .SetId("LandTween"))
            .Append(handTransform.DOLocalMove(defaultHandPosition, landReturnTime)
                .SetEase(Ease.InOutQuad)
                .SetId("LandTween"))
            .OnComplete(() =>
            {
                // Resume the move swing tween after landing if the player is grounded
                if (characterController != null && characterController.GroundCheck() && moveSwingTween != null)
                {
                    moveSwingTween.Play();
                }
            });
    }

    private void OnDisable()
    {
        S_PlayerStateObserver.Instance.OnShootStateEvent -= HandleShootEvent;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent -= HandleMeleeState;
        S_PlayerStateObserver.Instance.OnJumpStateEvent -= HandleJumpState;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent -= HandleGroundPoundState;
        S_PlayerStateObserver.Instance.OnMoveStateEvent -= HandleMoveEvent;
    }
}