using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class S_DamageModule : MonoBehaviour
{
    public float AdditionalDamage = 0f;   
    public float DamageMultiplier = 1f;   

    public event Action<float> OnDamage;

    public void ReceiveDamage(float baseDamage)
    {
        float finalDamage = (baseDamage + AdditionalDamage) * DamageMultiplier;
        Debug.Log("FinalDamage:" + finalDamage);
        OnDamage?.Invoke(finalDamage);
    }
}
