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
        if (p == null) {
            Debug.LogWarning("Player transform not found.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null && other.CompareTag("Player")) {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) {
                cc.enabled = false;
                p.position = target.transform.position;
                cc.enabled = true;
            }
        }
    }
}

