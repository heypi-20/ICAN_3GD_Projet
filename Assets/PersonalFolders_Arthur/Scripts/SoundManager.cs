using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.VisualScripting;

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
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
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
    
    public string Secret_surprise;
    private int actuallevel;


    #region Shoot
    [Header("Shoot")]
    public EventReference Classique_Shoot;
    private EventInstance Instance_Classique_Shoot;
    public EventReference Hit_Shoot;
    private EventInstance Instance_Hit_Shoot;
    public EventReference Weak_Shoot;
    private EventInstance Instace_Weak_Shoot;

    private void ShootState(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.ShootState.IsShooting :
                Instance_Classique_Shoot = RuntimeManager.CreateInstance(Classique_Shoot);
                Instance_Classique_Shoot.start();
                break;
            case PlayerStates.ShootState.hitEnemy :
                Instance_Hit_Shoot = RuntimeManager.CreateInstance(Hit_Shoot);
                Instance_Hit_Shoot.start();
                break;
            case PlayerStates.ShootState.hitWeakPoint : 
                Instance_Hit_Shoot = RuntimeManager.CreateInstance(Hit_Shoot);
                Instance_Hit_Shoot.start();
                Instace_Weak_Shoot = RuntimeManager.CreateInstance(Weak_Shoot);
                Instace_Weak_Shoot.start();
                break;
        }
    }
    

    #endregion

    #region Jump

    [Header("Jump")] 
    public EventReference Used_Jump;
    private EventInstance Instance_Used_Jump;
    public EventReference Toutched_Ground;
    private EventInstance Instance_Touched_Ground;
    private int Number_of_Jump;

    private void JumpState(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.JumpState.Jump :
                Instance_Used_Jump = RuntimeManager.CreateInstance(Used_Jump);
                Instance_Used_Jump.start();
                Number_of_Jump = Number_of_Jump + 1;
                Instance_Used_Jump.setParameterByName("Number_Of_Jump",Number_of_Jump);
                break;
            case PlayerStates.JumpState.OnGround :
                Instance_Touched_Ground = RuntimeManager.CreateInstance(Toutched_Ground);
                Instance_Touched_Ground.start();
                Number_of_Jump = 0;
                break;
        }
    }
    
    #endregion
    
    
    #region Sprint

    [Header("Sprint")]
    public EventReference Start_Sprint;
    private EventInstance Instance_StartSprint;
    public EventReference End_Sprint;
    private EventInstance Instance_EndSprint;

    private void SprintState(Enum state, int level)
    {
        switch (state)
        {
            case PlayerStates.SprintState.StartSprinting :
                Instance_StartSprint = RuntimeManager.CreateInstance(Start_Sprint);
                Instance_StartSprint.start();
                break;
            case PlayerStates.SprintState.StopSprinting :
                Instance_EndSprint = RuntimeManager.CreateInstance(End_Sprint);
                Instance_EndSprint.start();
                break;
            case PlayerStates.SprintState.SprintHit :
                Instance_Hit_Shoot = RuntimeManager.CreateInstance(Hit_Shoot);
                Instance_Hit_Shoot.start();
                break;
        }
    }

    #endregion

    #region Pillonage

    [Header("Pillonage")] 
    public EventReference ExplosionPillonage;
    private EventInstance Instance_ExplosionPillonage;
    public EventReference Fall_Pillonage;
    private EventInstance Instance_Fall_Pillonage;
    public EventReference Used_Pillonage;
    private EventInstance Instance_Used_Pillonage;

    private void PillonageState(Enum state)
    {
        switch (state)
        {
            case PlayerStates.GroundPoundState.StartGroundPound :
                Instance_Used_Pillonage = RuntimeManager.CreateInstance(Used_Pillonage);
                Instance_Used_Pillonage.start();
                break;
            case PlayerStates.GroundPoundState.isGroundPounding :
                Instance_Fall_Pillonage = RuntimeManager.CreateInstance(Fall_Pillonage);
                Instance_Fall_Pillonage.start();
                break;
            case PlayerStates.GroundPoundState.EndGroundPound :
                Instance_ExplosionPillonage = RuntimeManager.CreateInstance(ExplosionPillonage);
                Instance_ExplosionPillonage.start();
                Instance_Fall_Pillonage.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
        }
    }

    #endregion
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

    private void Start()
    {
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent += LevelChanged;
        S_PlayerStateObserver.Instance.OnMeleeAttackStateEvent += MeleeState;
        S_PlayerStateObserver.Instance.OnShootStateEvent += ShootState;
        S_PlayerStateObserver.Instance.OnSprintStateEvent += SprintState;
        S_PlayerStateObserver.Instance.OnJumpStateEvent += JumpState;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += PillonageState;
        Instance_Music_Lvl1 = RuntimeManager.CreateInstance(Music_Lvl1);
        Instance_Music_Lvl1.start();
    }
    
    #region LevelChanged
    [Header("Musique")]
    public EventReference Music_Lvl1;
    private EventInstance Instance_Music_Lvl1;
    public EventReference Music_Lvl2;
    private EventInstance Instance_Music_Lvl2;
    public EventReference Music_Lvl3;
    private EventInstance Instance_Music_Lvl3;
    public EventReference Music_Lvl4;
    private EventInstance Instance_Music_Lvl4;
    private void LevelChanged(Enum state,int Level)
    {
        switch (state)
        {
            case PlayerStates.LevelState.LevelDown when Level == 1 : 
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 0f);
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                Instance_Music_Lvl1.start();
                Instance_Music_Lvl2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            
            case PlayerStates.LevelState.LevelDown when Level == 2 : 
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 1f);
                Instance_Music_Lvl2.start();
                Instance_Music_Lvl3.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelDown when Level == 3 : 
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 2f);
                FMODUnity.RuntimeManager.PlayOneShot(Loose_Palier);
                Instance_Music_Lvl3.start();
                Instance_Music_Lvl4.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            
            
            
            case PlayerStates.LevelState.LevelUp when Level == 2 : 
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 1f);
                FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
                Instance_Music_Lvl2 = RuntimeManager.CreateInstance(Music_Lvl2);
                Instance_Music_Lvl2.start();
                Instance_Music_Lvl1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelUp when Level == 3 : 
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 2f);
                Instance_Music_Lvl3 = RuntimeManager.CreateInstance(Music_Lvl3);
                FMODUnity.RuntimeManager.PlayOneShot(Gain_Palier);
                Instance_Music_Lvl3.start();
                Instance_Music_Lvl2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                break;
            case PlayerStates.LevelState.LevelUp when Level == 4 : 
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("PalierInt", 3f);
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
    #endregion

    #region MeleeSound
    [Header("CAC")] 
    public EventReference Active_CAC;
    private EventInstance Instance_CAC_Active;
    public EventReference Used_CAC;
    private EventInstance Instance_CAC_Used;
    public EventReference Dash_CAC;
    private EventInstance Instance_Dash_CAC;
    private void MeleeState(Enum State,int Level)
    {
        switch (State)
        {
            case PlayerStates.MeleeState.StartMeleeAttack : 
                Instance_CAC_Active = RuntimeManager.CreateInstance(Active_CAC);
                Instance_CAC_Active.start();
                break;
            
            case PlayerStates.MeleeState.MeleeAttackHitWeakness : 
                Instance_Dash_CAC = RuntimeManager.CreateInstance(Dash_CAC);
                Instance_Dash_CAC.start();
                Instance_CAC_Used = RuntimeManager.CreateInstance(Used_CAC);
                Instance_CAC_Used.start();
                break;
            case PlayerStates.MeleeState.MeleeAttackHit :
                Instance_CAC_Used = RuntimeManager.CreateInstance(Used_CAC);
                Instance_CAC_Used.start();
                break;
        }
    }
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            FMODUnity.RuntimeManager.PlayOneShot(Secret_surprise);
        }
    }
}
