using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TriggerDestruction : MonoBehaviour
{
    public GameObject hitEffect;
    public Vector3 boxSize = Vector3.one;
    public LayerMask targetLayer;

    private void Update()
    {
        if (Physics.CheckBox(transform.position, boxSize * 0.5f, Quaternion.identity, targetLayer))
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}

