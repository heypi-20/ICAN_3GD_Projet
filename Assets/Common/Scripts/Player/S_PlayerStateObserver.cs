using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class S_PlayerStateObserver : MonoBehaviour
{
    public static S_PlayerStateObserver Instance;
    
    //Debugger UI
    [Header ("UI")]
    public TextMeshProUGUI stateText;
    private Queue<string> stateHistory = new Queue<string>();
    private const int maxHistory = 15;
    
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
        m_FireRateGun_Module.OnShootStateChange += OnShootStateChanged;
    }
    

    private void OnMoveStateChanged(Enum state, Vector2 direction)
    {
        //Done
        OnMoveStateEvent?.Invoke(state, direction);
        UpdateStateUI(state);
    }

    private void OnJumpStateChanged(Enum state)
    {
        //Done
        Debug.Log("OnJumpStateChanged"+state);
        UpdateStateUI(state);

    }

    private void OnSprintStateChanged(Enum state,int level)
    {
        //Done
        Debug.Log("OnSprintStateChanged: " + state+" level: " + level);
        UpdateStateUI(state);

    }

    private void OnShootStateChanged(Enum state,int level)
    {
        Debug.Log("OnShootStateChanged"+state+" level: "+level);
        UpdateStateUI(state);

    }

    private void OnMeleeStateChanged(Enum state)
    {
        
    }
    
    private void OnSpecialSkillStateChanged(Enum state)
    {
        
    }


    private void UpdateStateUI(Enum state)
    {// Convert enum to string
        string stateString = state.ToString();

        // Add the new state to the queue
        stateHistory.Enqueue(stateString);

        // Ensure only the last 5 states are stored
        if (stateHistory.Count > maxHistory)
        {
            stateHistory.Dequeue(); // Remove the oldest entry
        }

        // Update the UI text
        stateText.text = string.Join("\n", stateHistory);
    }
    
}
