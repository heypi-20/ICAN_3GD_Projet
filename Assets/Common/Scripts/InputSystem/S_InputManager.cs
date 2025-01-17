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
    

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        
        //Move action
        _playerInputActions.Gameplay.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        _playerInputActions.Gameplay.Move.canceled += ctx => MoveInput = Vector2.zero;
        
        //Jump action presse = true
        _playerInputActions.Gameplay.Jump.performed += ctx => JumpInput = true;
        
        
        //Sprint action presse = true, release = false
        _playerInputActions.Gameplay.Sprint.performed += ctx => SprintInput = true;
        _playerInputActions.Gameplay.Sprint.canceled += ctx => SprintInput = false;
        
        //Shoot Action
        _playerInputActions.Gameplay.Shoot.performed+= ctx => ShootInput = true;
        _playerInputActions.Gameplay.Shoot.canceled+=ctx=>ShootInput=false;
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
