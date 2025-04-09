using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public GameObject _shot;

    private void OnTriggerEnter(Collider other)
    {
        _shot.SetActive(true);
    }
    
    private void OnTriggerExit(Collider other)
    {
        _shot.SetActive(false);
    }
}
