using System;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerStateObserver : MonoBehaviour
{
    public static S_PlayerStateObserver Instance;
    
    // listening event
    private GameObject player; 
    private S_CustomCharacterController m_CharacterController;
    private S_BasicSprint_Module m_BasicSprint_Module;
    private S_BasicSpeedControl_Module m_BasicSpeedControl_Module;
    private S_EnergyStorage m_EnergyStorage;
    private S_EnergyAbsorption_Module m_EnergyAbsorption_Module;
    private S_SuperJump_Module m_SuperJump_Module;
    private S_FireRateGun_Module m_FireRateGun_Module;
    private S_MeleeAttack_Module m_MeleeAttack_Module;
    private S_GroundPound_Module m_GroundPound_Module;
    private S_PlayerHitTrigger m_PlayerHitTrigger;

    public Action<Enum, Vector2> OnMoveStateEvent;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Try to get player gamepbject
        player = FindObjectOfType<S_CustomCharacterController>()?.gameObject;
        if (player == null)
        {
            Debug.LogError("PlayerStateObserver:  Player not found ！");
            return;
        }

        // Try to get Skill Components
        m_CharacterController = player.GetComponent<S_CustomCharacterController>();
        m_BasicSprint_Module = player.GetComponent<S_BasicSprint_Module>();
        m_BasicSpeedControl_Module = player.GetComponent<S_BasicSpeedControl_Module>();
        m_EnergyStorage = player.GetComponent<S_EnergyStorage>();
        m_EnergyAbsorption_Module = player.GetComponent<S_EnergyAbsorption_Module>();
        m_SuperJump_Module = player.GetComponent<S_SuperJump_Module>();
        m_FireRateGun_Module = player.GetComponent<S_FireRateGun_Module>();
        m_MeleeAttack_Module = player.GetComponent<S_MeleeAttack_Module>();
        m_GroundPound_Module = player.GetComponent<S_GroundPound_Module>();
        m_PlayerHitTrigger = player.GetComponent<S_PlayerHitTrigger>();

        m_CharacterController.OnMoveStateChange += OnMoveStateChanged;
        m_BasicSprint_Module.OnSprintStateChange += OnSprintStateChanged;
        m_SuperJump_Module.OnJumpStateChange += OnJumpStateChanged;

    }
    

    private void OnMoveStateChanged(Enum state, Vector2 direction)
    {
        //Done
        OnMoveStateEvent?.Invoke(state, direction);
        Debug.Log("OnMoveStateChanged"+state);
    }

    private void OnJumpStateChanged(Enum state)
    {
        
        Debug.Log("OnJumpStateChanged"+state);
    }

    private void OnSprintStateChanged(Enum state,int level)
    {
        //Done
        Debug.Log("OnSprintStateChanged: " + state+" level: " + level);
    }

    private void OnShootStateChanged(Enum state)
    {
        
    }

    private void OnMeleeStateChanged(Enum state)
    {
        
    }
    
    private void OnSpecialSkillStateChanged(Enum state)
    {
        
    }
    
}
