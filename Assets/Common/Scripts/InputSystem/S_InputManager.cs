using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class S_InputManager : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    
    public Vector2 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool SprintInput { get; private set; }

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        
        //Move action
        _playerInputActions.Gameplay.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _playerInputActions.Gameplay.Move.canceled += ctx => MoveInput = Vector2.zero;
        
        //Jump action presse = true, release = false
        _playerInputActions.Gameplay.Jump.performed += ctx => JumpInput = true;
        _playerInputActions.Gameplay.Jump.canceled += ctx => JumpInput = false;
        
        //Sprint action presse = true, release = false
        _playerInputActions.Gameplay.Sprint.performed += ctx => SprintInput = true;
        _playerInputActions.Gameplay.Sprint.canceled += ctx => SprintInput = false;
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
