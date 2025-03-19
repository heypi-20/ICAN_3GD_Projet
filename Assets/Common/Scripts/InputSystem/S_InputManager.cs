using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_InputManager : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;

    [Header("Debugger")]
    public Vector2 MoveInput;
    public bool JumpInput;
    public bool SprintInput;
    public bool ShootInput;
    public bool MeleeAttackInput;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    
        // Move action
        _playerInputActions.Gameplay.Move.performed += OnMovePerformed;
        _playerInputActions.Gameplay.Move.canceled += ctx => MoveInput = Vector2.zero;
    
        // Jump action
        _playerInputActions.Gameplay.Jump.performed += OnJumpPerformed;
    
        // Sprint action
        _playerInputActions.Gameplay.Sprint.performed += OnSprintPerformed;
        _playerInputActions.Gameplay.Sprint.canceled += OnSprintCanceled;
    
        // Shoot action
        _playerInputActions.Gameplay.Shoot.performed += OnShootPerformed;
        _playerInputActions.Gameplay.Shoot.canceled += OnShootCanceled;
    
        // Melee attack input
        _playerInputActions.Gameplay.MeleeAttack.performed += OnMeleeAttackPerformed;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
            return;
    
        MoveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
            return;
    
        JumpInput = true;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
            return;
    
        SprintInput = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        SprintInput = false;
    }

    private void OnShootPerformed(InputAction.CallbackContext ctx)
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
            return;
    
        ShootInput = true;
    }

    private void OnShootCanceled(InputAction.CallbackContext ctx)
    {
        ShootInput = false;
    }

    private void OnMeleeAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
            return;
    
        MeleeAttackInput = true;
    }
    private void OnEnable()
    {
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
    }
}
