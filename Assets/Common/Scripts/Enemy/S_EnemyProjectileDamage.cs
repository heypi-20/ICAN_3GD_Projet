using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnemyProjectileDamage : MonoBehaviour
{
    public float damage;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<S_PlayerDamageReceiver>().ReceiveDamage(damage);

        }
    }
}
