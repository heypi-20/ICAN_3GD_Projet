using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SuperJumpModule : MonoBehaviour
{
    [Header("Jump Settings")]
    public float baseJumpForce = 5f; // Base jump force
    public float extraGravity = 2f; // Additional gravity multiplier

    [Header("Energy Settings")]
    public float baseEnergyConsumption = 10f; // Energy consumed for the first jump
    public float energyConsumptionMultiplier = 1.25f; // Multiplier for energy consumption per jump
    public float energyHeightMultiplier = 0.1f; // Multiplier for energy affecting jump height
    public float bonusHeightMultiplier = 0.5f; // Additional bonus height multiplier

    [Header("Key Bindings")]
    public KeyCode jumpKey = KeyCode.Space; // Key for jumping

    private Rigidbody rb;
    private float currentEnergyCost;
    private S_GroundCheck groundCheck;
    private S_EnergyStorage energyStorage; // Reference to energy storage system

    private void Start()
    {
        InitializeComponents();
    }

    private void Update()
    {
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        ApplyExtraGravity();
        ResetJumpStateIfGrounded();
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

        currentEnergyCost = baseEnergyConsumption;
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && CanJump())
        {
            Jump();
        }
    }

    private bool CanJump()
    {
        return energyStorage != null && energyStorage.currentEnergy >= currentEnergyCost;
    }

    private void ApplyExtraGravity()
    {
        if (!groundCheck.IsGrounded)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    private void ResetJumpStateIfGrounded()
    {
        if (groundCheck.IsGrounded)
        {
            currentEnergyCost = baseEnergyConsumption; // Reset energy cost when grounded
        }
    }

    private void Jump()
    {
        ResetVerticalVelocity();
        float bonusForce = CalculateBonusJumpForce();
        ApplyJumpForce(bonusForce);
        DeductEnergy();
        UpdateEnergyCost();
    }

    private void ResetVerticalVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = 0;
        rb.velocity = velocity;
    }

    private float CalculateBonusJumpForce()
    {
        if (energyStorage == null)
        {
            return 0f;
        }

        // Base jump force plus energy-based bonus
        return baseJumpForce + ((energyStorage.currentEnergy * energyHeightMultiplier) * bonusHeightMultiplier);
    }

    private void ApplyJumpForce(float bonusForce)
    {
        rb.AddForce(Vector3.up * bonusForce, ForceMode.Impulse);
    }

    private void DeductEnergy()
    {
        if (energyStorage != null)
        {
            energyStorage.currentEnergy = Mathf.Max(0, energyStorage.currentEnergy - currentEnergyCost);
        }
    }

    private void UpdateEnergyCost()
    {
        currentEnergyCost *= energyConsumptionMultiplier;
    }
}
