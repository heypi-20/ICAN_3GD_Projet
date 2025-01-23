using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_MultiJumpModule : MonoBehaviour
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
    private S_oldEnergyStorage _oldEnergyStorage; // Reference to energy storage system
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
        _oldEnergyStorage = GetComponent<S_oldEnergyStorage>();

        if (groundCheck == null)
        {
            Debug.LogError("S_GroundCheck component is missing on this GameObject!");
        }
        if (_oldEnergyStorage == null)
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
        return _oldEnergyStorage != null && _oldEnergyStorage.currentEnergy >= energyConsumption;
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
        if (_oldEnergyStorage == null)
        {
            return 0f;
        }

        float energyPercentage = _oldEnergyStorage.currentEnergy / (_oldEnergyStorage.maxEnergy > 0 ? _oldEnergyStorage.maxEnergy : 1f);
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
        if (_oldEnergyStorage != null)
        {
            _oldEnergyStorage.currentEnergy = Mathf.Max(0, _oldEnergyStorage.currentEnergy - energyConsumption);
        }
    }

    private void UpdateDynamicMaxJumps()
    {
        if (_oldEnergyStorage != null)
        {
            int additionalJumps = Mathf.FloorToInt(_oldEnergyStorage.currentEnergy / energyPerExtraJump);
            dynamicMaxJumps = maxJumps + additionalJumps;
        }
    }
}
