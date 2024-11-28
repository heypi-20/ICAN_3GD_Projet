using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCJump : MonoBehaviour
{
    private Rigidbody rb;
    public float _force = 10f;
    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            rb.AddForce(Vector3.up * _force, ForceMode.Impulse);
            rb.AddTorque(new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f)) * _force, ForceMode.Impulse);
            // rb.AddForce(transform.forward * _force, ForceMode.Impulse);
        }
    }
}
