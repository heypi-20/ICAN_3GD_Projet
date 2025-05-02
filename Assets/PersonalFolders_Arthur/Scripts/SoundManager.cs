using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{

    #region Check singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
                
                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(SoundManager).ToString());
                    _instance = singleton.AddComponent<SoundManager>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion
    [Header("Shoot_No_Hit")]
    [Space(10)]
    public string Shoot_No_Hit1;
    public string Shoot_No_Hit2;
    public string Shoot_No_Hit3;
    public string Shoot_No_Hit4;
    [Header("Shoot_Kill")]
    [Space(10)]
    public string Shoot_Kill1;
    public string Shoot_Kill2;
    public string Shoot_Kill3;
    public string Shoot_Kill4;
    
    [Header("CAC")] 
    public string CAC_Active;
    
    [Header("Jump")] 
    public string Used_Jump;
    
    [Header("Sprint")] 
    public string Active_Sprint;
    public string Desactive_Sprint;
    
    [Header("Pillonage")] 
    public string Pillonage_Explosion;
    public string Pillonage_Activate;
    public string Pillonage_Fall;
    
    [Header("Energy")] 
    public string Gain_Palier;
    public string Loose_Palier;
    public string Gain_Energy;
    
    [Header("Ennemy")] 
    public string Dashoot_Dash;
    public string Dashoot_Shoot;
    public string JumpyCuby_Jump;

    [Header("Musique")]
    public string Music_Lvl1;
    public string Music_Lvl2;
    public string Music_Lvl3;
    public string Music_Lvl4;
    public string Gain_Pallier;
    public string Loose_Pallier;
    private int actuallevel;
    public S_EnergyStorage energy_storage;
    
    //public S_Jauge_Tmp Jauge;
    public void Meth_Shoot_No_Hit(int currentLevel)
    {
        if (currentLevel == 1)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit1);
        }

        if (currentLevel == 2)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit2);
        }
        if (currentLevel == 3)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit3);
        }

        if (currentLevel == 4)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_No_Hit4);
        }
        
    }
    public void Meth_Shoot_Kill(int currentLevel)
    {
        if (currentLevel == 1)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_Kill1);
        }

        if (currentLevel == 2)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_Kill2);
        }
        if (currentLevel == 3)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_Kill3);
        }

        if (currentLevel == 4)
        {
            FMODUnity.RuntimeManager.PlayOneShot(Shoot_Kill4);
        }
    }

    public void Meth_Active_CAC()
    {
        FMODUnity.RuntimeManager.PlayOneShot(CAC_Active);
    }
    public void Meth_Used_Jump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Used_Jump);
    }
    public void Meth_Active_Sprint()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Active_Sprint);
    }
    public void Meth_Desactive_Sprint()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Desactive_Sprint);
    }
    public void Meth_Pillonage_Explosion()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Pillonage_Explosion);
    }
    public void Meth_Gain_Palier()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
    }
    public void Meth_Lose_Palier()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
    }
    public void Meth_Gain_Energy()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Gain_Energy);
    }
    public void Meth_Dashoot_Dash()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Dashoot_Dash);
    }
    public void Meth_Dashoot_Shoot()
    {
        FMODUnity.RuntimeManager.PlayOneShot(Dashoot_Shoot);
    }
    public void Meth_JumpyCuby_Jump()
    {
        FMODUnity.RuntimeManager.PlayOneShot(JumpyCuby_Jump);
    }

    public void SetParameter(float value)
    {
        // Modifier le param�tre global dans FMOD
        FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByName("Palier", value);
    }

    private void Update()
    {
        actuallevel = energy_storage.currentLevelIndex;
    }

    private void Start()
    {
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent += LevelChanged;
    }

    private void LevelChanged(Enum state,int Level)
    {
        switch (state)
        {
            case PlayerStates.LevelState.LevelUp when Level == 1 : 
                
                
                break;
        }
    }
}
