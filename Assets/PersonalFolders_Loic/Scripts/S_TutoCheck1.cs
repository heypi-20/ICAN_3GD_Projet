using System;
using UnityEngine;

public class S_TutoCheck1 : MonoBehaviour
{
    public int inputCount = 5;
    public int jumpCount = 5;

    public GameObject portal;
    
    private int icount;
    private int jcount;
    private bool movementComplete;
    private bool jumpComplete;

    private bool once = false;
    
    private void OnEnable()
    {
        FindObjectOfType<S_CustomCharacterController>().OnMoveStateChange += CheckForMovement;
        FindObjectOfType<S_SuperJump_Module>().OnJumpStateChange += CheckForJump;
    }

    private void OnDisable()
    {
        FindObjectOfType<S_CustomCharacterController>().OnMoveStateChange += CheckForMovement;
        FindObjectOfType<S_SuperJump_Module>().OnJumpStateChange += CheckForJump;
    }

    private void Start()
    {
        portal.SetActive(false);
    }

    private void Update()
    {
        if (movementComplete && jumpComplete && !once) {
            portal.SetActive(true);
            once = true;
        }
    }

    private void CheckForMovement(Enum playerState, Vector2 direction)
    {
        if (playerState.Equals(PlayerStates.MoveState.IsMoving)) {
            icount++;
        }

        if (icount == inputCount) {
            movementComplete = true;
        }
    }

    private void CheckForJump(Enum playerState, int level)
    {
        if (playerState.Equals(PlayerStates.JumpState.Jump)) {
            jcount++;
        }

        if (jcount == jumpCount) {
            jumpComplete = true;
        }
    }
}

