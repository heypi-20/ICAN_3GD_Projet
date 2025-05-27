using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHigh : MonoBehaviour
{
    public GameObject target;
    void Update()
    {
        if(gameObject.transform.position.y <50)
        {
            if (gameObject.GetComponent<CharacterController>() != null)
            {
                gameObject.GetComponent<CharacterController>().enabled = false;
                gameObject.transform.position = target.transform.position;
                gameObject.GetComponent<CharacterController>().enabled = true;
            }
        }
    }
}
