using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump_Noeud : MonoBehaviour
{
    public S_PlayerController PlayerController;
    private GameObject Player;
    public float Jump_force;
    
    public float time_to_survive;
    
    private void OnTriggerStay(Collider other)
    {
        PlayerController.jumpForce = Jump_force;
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController.jumpForce = 0;
    }

    private void FixedUpdate()
    {
        time_to_survive -= Time.deltaTime;

        if (time_to_survive <= 0)
        {
            PlayerController.jumpForce = 0;
            Debug.Log("timer end");
            time_to_survive = 0;
        }
        
    }

    private void Start()
    {
        Player = GameObject.FindWithTag("Player");
    }
}
