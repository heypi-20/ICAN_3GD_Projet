using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attention_grenade : MonoBehaviour
{
    public Vector3 addforce;
    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(addforce, ForceMode.Impulse);
    }
}
