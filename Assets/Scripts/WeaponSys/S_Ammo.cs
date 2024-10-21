using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Ammo : MonoBehaviour
{
    public int ammo = 0;
    
    private S_PlayerController player;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<S_PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.tag == "Cube") {
            Destroy(other.gameObject);
            ammo++;
        }
    }
}
