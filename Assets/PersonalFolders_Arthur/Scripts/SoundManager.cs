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
    public EventReference Active_CAC;
    private EventInstance Instance_CAC_Active;
    public EventReference Used_CAC;
    private EventInstance Instance_CAC_Used;
    public EventReference Dash_CAC;
    private EventInstance Instance_Dash_CAC;
    
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
    public EventReference Music_Lvl1;
    private EventInstance Instance_Music_Lvl1;
    public EventReference Music_Lvl2;
    private EventInstance Instance_Music_Lvl2;
    public EventReference Music_Lvl3;
    private EventInstance Instance_Music_Lvl3;
    public EventReference Music_Lvl4;
    private EventInstance Instance_Music_Lvl4;
    
    
    private int actuallevel;
    //public S_EnergyStorage energy_storage;
    
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
    public void Meth_Used_CAC()
    {
        Instance_CAC_Used = RuntimeManager.CreateInstance(Used_CAC);
        Instance_CAC_Used.start();
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

    private void Start()
    {
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent += LevelChanged;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += MeleeState;
        Instance_Music_Lvl1 = RuntimeManager.CreateInstance(Music_Lvl1);
        Instance_Music_Lvl1.start();
    }

    private void LevelChanged(Enum state,int Level)
    {
        switch (state)
        {
            case PlayerStates.LevelState.LevelDown when Level == 1 : 
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                Instance_Music_Lvl1.start();
                Instance_Music_Lvl2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            
            case PlayerStates.LevelState.LevelDown when Level == 2 : 
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                Instance_Music_Lvl2.start();
                Instance_Music_Lvl3.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelDown when Level == 3 : 
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                Instance_Music_Lvl3.start();
                Instance_Music_Lvl4.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            
            
            
            case PlayerStates.LevelState.LevelUp when Level == 2 : 
                FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
                Instance_Music_Lvl2 = RuntimeManager.CreateInstance(Music_Lvl2);
                Instance_Music_Lvl2.start();
                Instance_Music_Lvl1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelUp when Level == 3 : 
                Instance_Music_Lvl3 = RuntimeManager.CreateInstance(Music_Lvl3);
                FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
                Instance_Music_Lvl3.start();
                Instance_Music_Lvl2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelUp when Level == 4 : 
                Instance_Music_Lvl4 = RuntimeManager.CreateInstance(Music_Lvl4);
                FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
                Instance_Music_Lvl4.start();
                Instance_Music_Lvl3.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.StartGrace when Level == 2:
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl2, 1f, 0f, 5f));
                break;

            case PlayerStates.LevelState.EndGrace when Level == 2:
                StopAllCoroutines();
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl2, 0f, 1f, 0.2f));
                break;

            case PlayerStates.LevelState.StartGrace when Level == 3:
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl3, 1f, 0f, 5f));
                break;

            case PlayerStates.LevelState.EndGrace when Level == 3:
                StopAllCoroutines();
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl3, 0f, 1f, 0.2f));
                break;

            case PlayerStates.LevelState.StartGrace when Level == 4:
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl4, 1f, 0f, 5f));
                break;

            case PlayerStates.LevelState.EndGrace when Level == 4:
                StopAllCoroutines();
                StartCoroutine(ChangePitchOverTime(Instance_Music_Lvl4, 0f, 1f, 0.2f));
                break;
        }
        IEnumerator ChangePitchOverTime(EventInstance musicInstance, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float value = Mathf.Lerp(from, to, elapsed / duration);
                musicInstance.setParameterByName("Pitch_Music", value);
                yield return null;
            }
            musicInstance.setParameterByName("Pitch_Music", to); // Assure la valeur finale
        }
    }


    private void MeleeState(Enum State,int Level)
    {
        switch (State)
        {
            case PlayerStates.MeleeState.StartMeleeAttack : 
                Instance_CAC_Active = RuntimeManager.CreateInstance(Active_CAC);
                Instance_CAC_Active.start();
                break;
            
            case PlayerStates.MeleeState.DashingBeforeMelee : 
                Instance_Dash_CAC = RuntimeManager.CreateInstance(Dash_CAC);
                Instance_Dash_CAC.start();
                break;
            case PlayerStates.MeleeState.MeleeAttackHit :
                Debug.Log("HitHithit");
                Instance_CAC_Used = RuntimeManager.CreateInstance(Used_CAC);
                Instance_CAC_Used.start();
                break;
        }
    }
}
