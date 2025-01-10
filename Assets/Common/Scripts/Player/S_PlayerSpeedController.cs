using System;
using UnityEngine;

public class S_PlayerSpeedController : MonoBehaviour
{
    private S_PlayerMultiCam pc;
    private S_oldEnergyStorage _oldEnergyStorage;
    private Rigidbody rb;

    private void Start()
    {
        pc = GetComponent<S_PlayerMultiCam>();
        _oldEnergyStorage = GetComponent<S_oldEnergyStorage>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        pc.moveSpeed = CalculateMoveSpeed();
    }

    private float CalculateMoveSpeed()
    {
        if (pc.enableEnergyBoost && _oldEnergyStorage != null)
        {
            float energyBoost = _oldEnergyStorage.currentEnergy * pc.energyPercentageIncrease * pc.multiplier;
            return pc.baseSpeed + energyBoost;
        }
        return pc.baseSpeed;
    }
    
    public void SpeedControl()
    {
        if (pc.OnSlope())
        {
            if (rb.velocity.magnitude > pc.moveSpeed)
                rb.velocity = rb.velocity.normalized * pc.moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > pc.moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * pc.moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
}

