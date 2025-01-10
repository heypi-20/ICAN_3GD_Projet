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
    private S_oldEnergyStorage _oldEnergyStorage; // Reference to energy storage system

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
        _oldEnergyStorage = GetComponent<S_oldEnergyStorage>();

        if (groundCheck == null)
        {
            Debug.LogError("S_GroundCheck component is missing on this GameObject!");
        }
        if (_oldEnergyStorage == null)
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
        return _oldEnergyStorage != null && _oldEnergyStorage.currentEnergy >= currentEnergyCost;
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
        if (_oldEnergyStorage == null)
        {
            return 0f;
        }

        // Base jump force plus energy-based bonus
        return baseJumpForce + ((_oldEnergyStorage.currentEnergy * energyHeightMultiplier) * bonusHeightMultiplier);
    }

    private void ApplyJumpForce(float bonusForce)
    {
        rb.AddForce(Vector3.up * bonusForce, ForceMode.Impulse);
    }

    private void DeductEnergy()
    {
        if (_oldEnergyStorage != null)
        {
            _oldEnergyStorage.currentEnergy = Mathf.Max(0, _oldEnergyStorage.currentEnergy - currentEnergyCost);
        }
    }

    private void UpdateEnergyCost()
    {
        currentEnergyCost *= energyConsumptionMultiplier;
    }
}
