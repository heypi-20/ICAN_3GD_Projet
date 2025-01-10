using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;
    
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump=true;
    
    public KeyCode jumpKey = KeyCode.Space;
    
    [Header("Slope Handling")]
    public float maxslopeAngle;
    private RaycastHit slopeHit;
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer;
    private bool grounded;
    
    public Transform orientation;
    
    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        //check grounded
        grounded = Physics.Raycast(transform.position,Vector3.down,playerHeight*0.5f+0.2f,groundLayer);
        
        MyInput();
        SpeedControl();
        
        // Ground Drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        //movement input
        horizontalInput = Input.GetAxis("Horizontal");  
        verticalInput = Input.GetAxis("Vertical");
        
        //jump input
        if (Input.GetKeyDown(jumpKey) && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        //Calculate Move direction
        moveDirection = orientation.forward * verticalInput+orientation.right * horizontalInput;
        
        //On slope
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * (moveSpeed * 20f), ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down*80f, ForceMode.Force);
            }
        }

        if (grounded)
            rb.AddForce(moveDirection.normalized * (moveSpeed * 10f), ForceMode.Force);
        else if (!grounded) rb.AddForce(moveDirection.normalized * (moveSpeed * 10f * airMultiplier), ForceMode.Force);
    }

    private void SpeedControl()
    {
        if (OnSlope())
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatvel= new Vector3(rb.velocity.x, 0, rb.velocity.z);
        
            //limite velocity if needed
            if (flatvel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatvel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        
        
    }

    private void Jump()
    {
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x,0, rb.velocity.z);
        
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(transform.up, slopeHit.normal);
            return angle < maxslopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
