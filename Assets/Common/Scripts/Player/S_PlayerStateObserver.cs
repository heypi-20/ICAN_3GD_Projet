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
    private S_PlayerDamageReceiver m_PlayerDamageReceiver;

    public Action<Enum, Vector2> OnMoveStateEvent;
    public Action<Enum, int> OnMeleeAttackStateEvent;
    public Action<Enum, int> OnShootStateEvent;
    public Action<Enum,int> OnJumpStateEvent;
    public Action<Enum,int> OnGroundPoundStateEvent;
    public Action<Enum,int> OnLevelUpStateEvent;
    public Action<Enum,int> OnSprintStateEvent; 
    public Action<Enum> OnPlayerHealthStateEvent;

    public Enum LastMeleeState;
    public Enum LastGroundPoundState;
    
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
        m_PlayerDamageReceiver = player.GetComponent<S_PlayerDamageReceiver>();

        m_CharacterController.OnMoveStateChange += OnMoveStateChanged;
        m_BasicSprint_Module.OnSprintStateChange += OnSprintStateChanged;
        m_SuperJump_Module.OnJumpStateChange += OnJumpStateChanged;
        m_FireRateGun_Module.OnShootStateChange += OnShootStateChanged;
        m_EnergyStorage.OnLevelChange += OnLevelStateChange;
        m_MeleeAttack_Module.OnAttackStateChange += OnMeleeStateChanged;
        m_GroundPound_Module.OnGroundPoundStateChange += OnSpecialSkillStateChanged;
        m_PlayerDamageReceiver.OnPlayerHealthState+=OnPlayerHealthStateChanged;
    }

    private void OnPlayerHealthStateChanged(Enum state)
    {
        OnPlayerHealthStateEvent?.Invoke(state);
        UpdateStateUI(state);

    }

    private void OnMoveStateChanged(Enum state, Vector2 direction)
    {
        //Done
        OnMoveStateEvent?.Invoke(state, direction);
        UpdateStateUI(state);
    }

    private void OnJumpStateChanged(Enum state, int level)
    {
        //Done
        UpdateStateUI(state);
        OnJumpStateEvent?.Invoke(state,level);

    }

    private void OnSprintStateChanged(Enum state,int level)
    {
        //Done
        UpdateStateUI(state);
        OnSprintStateEvent?.Invoke(state, level);
    }

    private void OnShootStateChanged(Enum state,int level)
    {
        UpdateStateUI(state);
        OnShootStateEvent?.Invoke(state, level);

    }

    private void OnLevelStateChange(Enum state, int level)
    {
        UpdateStateUI(state,level);
        OnLevelUpStateEvent?.Invoke(state, level);
    }

    private void OnMeleeStateChanged(Enum state, int level)
    {
        OnMeleeAttackStateEvent?.Invoke(state, level);
        UpdateStateUI(state);
        //Use for shoot logic, to not allow shooting and punching on the same time
        LastMeleeState = state.Equals(PlayerStates.MeleeState.EndMeleeAttack) ? null : state;
        
    }
    
    private void OnSpecialSkillStateChanged(Enum state,int level)
    {
        OnGroundPoundStateEvent?.Invoke(state,level);
        UpdateStateUI(state);
        LastGroundPoundState = state.Equals(PlayerStates.GroundPoundState.EndGroundPound) ? null : state;
    }


    private void UpdateStateUI(Enum state, float? levelValue = null)
    {
        // Convert the enum to its name
        string stateString = state.ToString();

        // If a level value was provided, append it
        if (levelValue.HasValue)
        {
            // If the value is whole, drop the “.0”; otherwise show up to two decimals
            string levelText = levelValue.Value % 1 == 0
                ? ((int)levelValue.Value).ToString()
                : levelValue.Value.ToString("0.##");
            stateString += $" (Level: {levelText})";
        }

        // Enqueue the new state string
        stateHistory.Enqueue(stateString);

        // Keep only the most recent maxHistory entries
        if (stateHistory.Count > maxHistory)
        {
            stateHistory.Dequeue();
        }

        // Update the UI text field
        stateText.text = string.Join("\n", stateHistory);
    }
    
}
