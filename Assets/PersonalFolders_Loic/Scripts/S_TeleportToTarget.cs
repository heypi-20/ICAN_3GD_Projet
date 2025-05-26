using System;
using UnityEngine;

public class S_TeleportToTarget : MonoBehaviour
{
    public GameObject target;

    private Transform p;
    private void Start()
    {
        if (target == null) {
            Debug.LogWarning("Target object is null");
            return;
        }

        p = FindObjectOfType<S_CustomCharacterController>().transform;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (target != null && other.gameObject.CompareTag("Player")) {
            p.position = target.transform.position;
        }
    }
}

