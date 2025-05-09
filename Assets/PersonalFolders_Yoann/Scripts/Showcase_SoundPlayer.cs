using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

public class Showcase_SoundPlayer : MonoBehaviour
{
    public string _eventName;
    
    private void OnTriggerEnter(Collider other)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_eventName);
    }
}
