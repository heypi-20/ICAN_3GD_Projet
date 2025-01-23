using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    [Header("Shoot_No_Hit")]
    public string Shoot_No_Hit1;
    public string Shoot_No_Hit2;
    public string Shoot_No_Hit3;
    public string Shoot_No_Hit4;

    public void Meth_Shoot_No_Hit_Lvl1()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit1);
    }
    public void Meth_Shoot_No_Hit_Lvl2()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit2);
    }
    public void Meth_Shoot_No_Hit_Lvl3()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit3);
    }
    public void Meth_Shoot_No_Hit_Lvl4()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit4);
    }
    
}
