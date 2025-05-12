using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class S_PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform; // The transform of the FPS character's hand/weapon
    public Animator handAnimator;

    [Header("Jump Animation Settings")]
    public float jumpOffsetY = -0.2f; // Downward offset when jumping
    public float jumpOffsetZ = 0.1f;  // Forward offset when jumping
    public float jumpDropTime = 0.05f; // Time for the hand to drop quickly
    public float jumpReturnTime = 0.3f; // Time for the hand to slowly return

    [Header("Landing Animation Settings")]
    // These values are defaults; actual values can be adjusted dynamically based on air time
    public float landBounceTime = 0.05f; // Quick upward bounce time
    public float landReturnTime = 0.15f; // Smooth return time to default position

    [Header("Movement Swing Settings")]
    public Vector3 swingAmplitude = new Vector3(0.05f, 0.05f, 0f); // Swing amplitude when moving
    public float swingDuration = 0.5f; // Duration for half a swing cycle
    public Ease swingEase = Ease.InOutSine; // Ease type for natural acceleration and deceleration

    [Header("Air Time Landing Settings")]
    public float minAirTimeForLandingAnimation = 0.3f; // Minimum air time to trigger landing animation
    public float landMinOffsetY = 0.1f; // Minimum upward offset for landing animation
    public float landMaxOffsetY = 0.3f; // Maximum upward offset for landing animation
    public float maxAirTimeForMaxLanding = 1.0f; // Air time above which maximum offset is used

    private Vector3 defaultHandPosition; // Default local position of the hand
    private Tween moveSwingTween; // Tween for the hand swing animation during movement
    private Tween jumpTween;      // Tween for the jump hand animation

    private int skillLayerIndex;

    // Reference to the custom character controller for ground checking
    private S_CustomCharacterController characterController;

    // Flag to track if the player is currently moving
    private bool isMoving = false;

    // Used to record the time when the player goes airborne
    private float airStartTime = 0f;

    private void Start()
    {
        skillLayerIndex = handAnimator.GetLayerIndex("SkillLayer");
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
        // When the player is moving, check if they are grounded.
        // If not, pause the hand swing tween; if grounded and paused, resume it.
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
            case PlayerStates.MoveState.StartMoving when moveSwingTween == null:
                isMoving = true;
                StartHandSwing();
                break;
            case PlayerStates.MoveState.StopMoving:
                isMoving = false;
                StopHandSwing();
                break;
        }
    }

    // Start the hand swing animation (using DOTween's Yoyo loop)
    private void StartHandSwing()
    {
        // Only start the swing if the player is grounded
        if (characterController != null && !characterController.GroundCheck())
        {
            return;
        }
        // Kill any existing tween to avoid duplicates
        if (moveSwingTween != null && moveSwingTween.IsActive())
        {
            moveSwingTween.Kill();
        }
        // Create a looping tween that moves the hand from default to an offset position and back
        moveSwingTween = handTransform.DOLocalMove(defaultHandPosition + swingAmplitude, swingDuration)
            .SetEase(swingEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId("MoveSwingTween");
    }

    // Stop the hand swing animation and smoothly return the hand to its default position
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
                handAnimator.CrossFade("StartShoot", 0.2f);
                break;
            // case PlayerStates.ShootState.IsShooting when S_PlayerStateObserver.Instance.LastMeleeState == null:
            //     handAnimator.CrossFade("HoldShoot", 0.2f);
            //     break;
            case PlayerStates.ShootState.StopShoot when S_PlayerStateObserver.Instance.LastMeleeState == null:
                handAnimator.CrossFade("EndShoot", 0.2f);
                break;
        }
    }

    private void HandleMeleeState(Enum MeleeState, int Level)
    {
        switch (MeleeState)
        {
            case PlayerStates.MeleeState.StartMeleeAttack when S_PlayerStateObserver.Instance.LastGroundPoundState == null:
                handAnimator.CrossFade("Punch", 0.2f);
                break;
        }
    }

    // Modified jump state handling with an additional OnAir state
    private void HandleJumpState(Enum JumpState, int level)
    {
        switch (JumpState)
        {
            case PlayerStates.JumpState.Jump:
                // Record the time when the player goes airborne
                // handAnimator.SetLayerWeight(skillLayerIndex, 1f);
                // handAnimator.Play("UseRune", skillLayerIndex, 0f);
                JumpHandAnimation();
                break;
            case PlayerStates.JumpState.OnAir:
                airStartTime = Time.time;
                break;
            case PlayerStates.JumpState.OnGround:
                // Calculate the air duration
                float airDuration = Time.time - airStartTime;
                handAnimator.SetLayerWeight(skillLayerIndex, 0f);
                if (airDuration >= minAirTimeForLandingAnimation)
                {
                    // Interpolate between the minimum and maximum landing offset based on air time
                    float t = Mathf.Clamp01((airDuration - minAirTimeForLandingAnimation) / (maxAirTimeForMaxLanding - minAirTimeForLandingAnimation));
                    float adjustedLandingOffsetY = Mathf.Lerp(landMinOffsetY, landMaxOffsetY, t);
                    ResetHandPositionWhenLanding(adjustedLandingOffsetY);
                }
                else
                {
                    // If air time is too short, simply reset the hand position
                    handTransform.DOLocalMove(defaultHandPosition, 0.2f).SetEase(Ease.OutQuad);
                }
                break;
        }
    }

    private void HandleGroundPoundState(Enum GroundPoundState,int Level)
    {
        switch (GroundPoundState)
        {
            case PlayerStates.GroundPoundState.StartGroundPound:
                handAnimator.CrossFade("StartGroundPound", 0.2f);
                break;
            // case PlayerStates.GroundPoundState.isGroundPounding:
            //     handAnimator.CrossFade("HoldGroundPound", 0.2f);
            //     break;
            case PlayerStates.GroundPoundState.EndGroundPound:
                handAnimator.CrossFade("EndGroundPound", 0.2f);
                break;
        }
    }

    // Jump animation sequence remains unchanged
    private void JumpHandAnimation()
    {
        // Pause the movement swing tween to avoid conflicts
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
                // Resume the movement swing tween if the player is grounded
                if (characterController != null && characterController.GroundCheck() && moveSwingTween != null)
                {
                    moveSwingTween.Play();
                }
            });
    }

    // Modified landing animation method that takes the landing offset as a parameter
    private void ResetHandPositionWhenLanding(float landingOffsetY)
    {
        // Pause the movement swing tween to avoid conflicts
        if (moveSwingTween != null && moveSwingTween.IsActive())
        {
            moveSwingTween.Pause();
        }
    
        Tween landTween = DOTween.Sequence()
            .Append(handTransform.DOLocalMove(defaultHandPosition + new Vector3(0, landingOffsetY, 0), landBounceTime)
                .SetEase(Ease.OutQuad)
                .SetId("LandTween"))
            .Append(handTransform.DOLocalMove(defaultHandPosition, landReturnTime)
                .SetEase(Ease.InOutQuad)
                .SetId("LandTween"))
            .OnComplete(() =>
            {
                // Explicitly reset the hand position to default
                handTransform.localPosition = defaultHandPosition;
                // Restart the movement swing tween if the player is grounded and still moving
                if (characterController != null && characterController.GroundCheck() && isMoving)
                {
                    if (moveSwingTween != null)
                    {
                        moveSwingTween.Kill();
                        moveSwingTween = null;
                    }
                    StartHandSwing();
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
