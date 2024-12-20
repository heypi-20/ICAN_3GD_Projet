using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump_Noeud : MonoBehaviour
{
    private S_PlayerController PlayerController;
    private GameObject playerObject;
    public float Jump_force;
    
    public float time_to_survive;

    public bool Add_Jump = true;
    
    private void OnTriggerStay(Collider other)
    {

        if (Add_Jump == false)
        {
            PlayerController.jumpForce = Jump_force;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Add_Jump == true)
        {
            Debug.Log("111111111111111");
            PlayerController.jumpForce += Jump_force;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Add_Jump == true)
        {
            Debug.Log("222");
            PlayerController.jumpForce -= Jump_force;
        }

        if (Add_Jump == false)
        {
            PlayerController.jumpForce = 0;
        }
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
        S_PlayerController playerObject = FindObjectOfType<S_PlayerController>();
        if (playerObject != null)
        {
            PlayerController = playerObject.GetComponent<S_PlayerController>();
            if (PlayerController == null)
            {
                Debug.LogError("Le script S_PlayerController n'est pas attaché à l'objet avec le tag 'Player'.");
            }
        }
        else
        {
            Debug.LogError("Aucun objet avec le tag 'Player' n'a été trouvé dans la scène.");
        }
    }
}
