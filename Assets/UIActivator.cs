using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActivator : MonoBehaviour
{
    public GameObject _canva;
    private bool _activated = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(_activated);
            _activated = !_activated;
            _canva.SetActive(_activated);
        }
    }
}
