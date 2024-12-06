using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class attention_grenade : MonoBehaviour
{
    public Vector3 addforce;
    private float Timer;
    public float time_todeploy;
    public GameObject Jump_Noeud;

    private void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // Appliquer la force en prenant en compte l'orientation de l'objet au moment du spawn
        Vector3 force = transform.rotation * addforce;
        rb.AddForce(force, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        Timer += Time.deltaTime;
        if (time_todeploy <= Timer)
        {
            Instantiate(Jump_Noeud, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}

