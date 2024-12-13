using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_JumpModule : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 5f; // Base jump force
    public int maxJumps = 2; // Base maximum number of jumps allowed
    public float extraGravity = 2f; // Additional gravity multiplier

    [Header("Energy Settings")]
    public float energyConsumption = 10f; // Energy consumed per jump
    public float energyBonusMultiplier = 0.1f; // Bonus force per 10% of current energy
    public float energyPerExtraJump = 50f; // Energy required to gain one additional jump

    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space; // Key for jumping

    private Rigidbody rb;
    private int currentJumps;
    private int dynamicMaxJumps;
    private S_GroundCheck groundCheck;
    private S_EnergyStorage energyStorage; // Reference to energy storage system
    private float bonusJumpForce;
    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandleJumpInput();
        UpdateDynamicMaxJumps();
    }

    private void FixedUpdate()
    {
        ApplyExtraGravity();
        ResetJumpCountIfGrounded();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponent<S_GroundCheck>();
        energyStorage = GetComponent<S_EnergyStorage>();

        if (groundCheck == null)
        {
            Debug.LogError("S_GroundCheck component is missing on this GameObject!");
        }
        if (energyStorage == null)
        {
            Debug.LogError("S_EnergyStorage component is missing on this GameObject!");
        }

        currentJumps = 0;
        dynamicMaxJumps = maxJumps; // Start with the base max jumps
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && (groundCheck.IsGrounded || currentJumps < dynamicMaxJumps - 1))
        {
            if (HasSufficientEnergy())
            {
                Jump();
            }
            else
            {
                Debug.Log("Not enough energy to jump!");
            }
        }
    }

    private void ApplyExtraGravity()
    {
        if (!groundCheck.IsGrounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    private void ResetJumpCountIfGrounded()
    {
        if (groundCheck.IsGrounded)
        {
            currentJumps = 0;
        }
    }

    private bool HasSufficientEnergy()
    {
        return energyStorage != null && energyStorage.currentEnergy >= energyConsumption;
    }

    private void Jump()
    {
        ResetVerticalVelocity();
        float bonusForce = CalculateEnergyBonus();
        ApplyJumpForce(bonusForce);
        bonusJumpForce = bonusForce;
        DeductEnergy();
        currentJumps++;
    }

    private float CalculateEnergyBonus()
    {
        if (energyStorage == null)
        {
            return 0f;
        }

        float energyPercentage = energyStorage.currentEnergy / (energyStorage.maxEnergy > 0 ? energyStorage.maxEnergy : 1f);
        return energyPercentage * energyBonusMultiplier * jumpForce;
    }

    private void ResetVerticalVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
    }

    private void ApplyJumpForce(float bonusForce)
    {
        rb.AddForce(Vector3.up * (jumpForce + bonusForce), ForceMode.Impulse);
    }

    private void DeductEnergy()
    {
        if (energyStorage != null)
        {
            energyStorage.currentEnergy = Mathf.Max(0, energyStorage.currentEnergy - energyConsumption);
        }
    }

    private void UpdateDynamicMaxJumps()
    {
        if (energyStorage != null)
        {
            int additionalJumps = Mathf.FloorToInt(energyStorage.currentEnergy / energyPerExtraJump);
            dynamicMaxJumps = maxJumps + additionalJumps;
        }
    }
}
