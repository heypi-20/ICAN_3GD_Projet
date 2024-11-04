using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class S_DamageModule : MonoBehaviour
{
    // Public fields for configuring damage calculation
    public float AdditionalDamage = 0f;   // Extra damage value added to base damage
    public float DamageMultiplier = 1f;   // Multiplier applied to base damage

    // Event that triggers after damage is calculated
    public event Action<float> OnDamage;

    //public S_PhysicsCollisionModule TouchEventScript;

    //private void OnEnable()
    //{
    //    if (TouchEventScript != null)
    //    {
    //        TouchEventScript.OnTouch += () => ReceiveDamage(10f);
    //    }
    //}

    //private void OnDisable()
    //{
    //    if (TouchEventScript != null)
    //    {
    //        TouchEventScript.OnTouch -= () => ReceiveDamage(10f);
    //    }
    //}


    // Method to handle receiving damage
    public void ReceiveDamage(float baseDamage)
    {
        // Calculate final damage using AdditionalDamage and DamageMultiplier
        float finalDamage = (baseDamage + AdditionalDamage) * DamageMultiplier;
        Debug.Log("FinalDamage:" + finalDamage);
        // Trigger the OnDamage event with the calculated damage value
        OnDamage?.Invoke(finalDamage);
    }
}
