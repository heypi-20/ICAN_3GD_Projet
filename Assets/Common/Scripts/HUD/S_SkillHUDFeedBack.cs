using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class S_SkillHUDFeedBack : MonoBehaviour
{
    [Header("Crystal Icons by Level")]
    [Tooltip("Level 1: Jump Crystal")]
    [SerializeField] private GameObject jumpCrystal;

    [Tooltip("Level 2: Sprint Crystal")]
    [SerializeField] private GameObject sprintCrystal;

    [Tooltip("Level 3: Ground Pound Crystal")]
    [SerializeField] private GameObject groundPoundCrystal;

    // Map from level number to corresponding crystal GameObject
    private Dictionary<int, GameObject> levelCrystalMap;

    // Store each crystal's original localScale so we can restore it after shaking
    private Dictionary<int, Vector3> originalScales;

    // Current infinite shake tween for grace warning
    private Tween graceTween;
    private int graceLevel = -1;

    private void Awake()
    {
        // Initialize the level-to-crystal mapping
        levelCrystalMap = new Dictionary<int, GameObject>
        {
            { 1, jumpCrystal },
            { 2, sprintCrystal },
            { 3, groundPoundCrystal }
        };

        // Record original scales
        originalScales = new Dictionary<int, Vector3>();
        foreach (var kv in levelCrystalMap)
        {
            if (kv.Value != null)
                originalScales[kv.Key] = kv.Value.transform.localScale;
        }
    }

    private void Start()
    {
        // Subscribe to level-related events
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent     += HandleLevelChangeEvent;
        // Optional: subscribe to skill events for extra feedback
        S_PlayerStateObserver.Instance.OnJumpStateEvent        += HandleJumpStateEvent;
        S_PlayerStateObserver.Instance.OnSprintStateEvent      += HandleSprintStateEvent;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += HandleGroundPoundStateEvent;
    }

    /// <summary>
    /// Handle LevelUp, StartGrace (pre-downgrade shake), EndGrace, and LevelDown.
    /// </summary>
    private void HandleLevelChangeEvent(Enum state, int level)
    {
        var lvlState = (PlayerStates.LevelState)state;
        switch (lvlState)
        {
            case PlayerStates.LevelState.LevelUp:
                // Activate all crystals <= current level and stop any ongoing grace shake
                KillGraceTween();
                foreach (var kv in levelCrystalMap)
                    kv.Value.SetActive(kv.Key <= level);
                break;

            case PlayerStates.LevelState.StartGrace:
                // Start infinite shake on the crystal for the current level
                StartGraceShake(level);
                break;

            case PlayerStates.LevelState.EndGrace:
                // Stop the shake and restore original scale
                KillGraceTween();
                break;

            case PlayerStates.LevelState.LevelDown:
                // Stop the shake, then deactivate all crystals > current level
                KillGraceTween();
                foreach (var kv in levelCrystalMap)
                    if (kv.Key > level)
                        kv.Value.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// Starts an infinite shake tween on the specified level's crystal.
    /// </summary>
    private void StartGraceShake(int level)
    {
        // Kill any existing grace tween
        KillGraceTween();

        if (!levelCrystalMap.TryGetValue(level, out var crystal) || crystal == null)
            return;

        graceLevel = level;
        // Shake scale indefinitely, restarting each loop to original scale
        graceTween = crystal.transform
            .DOShakeScale(
                duration:   0.5f,
                strength:   new Vector3(0.2f, 0.2f, 0.2f),
                vibrato:    10,
                randomness: 90
            )
            .SetLoops(-1, LoopType.Restart)
            .SetAutoKill(false);
    }

    /// <summary>
    /// Stops the current shake tween and restores the original scale.
    /// </summary>
    private void KillGraceTween()
    {
        if (graceTween != null)
        {
            graceTween.Kill();
            if (graceLevel > 0 
                && levelCrystalMap.TryGetValue(graceLevel, out var oldCrystal) 
                && originalScales.TryGetValue(graceLevel, out var origScale))
            {
                oldCrystal.transform.localScale = origScale;
            }
            graceTween = null;
            graceLevel = -1;
        }
    }

    /// <summary>
    /// Optional: punch-scale feedback when player jumps.
    /// </summary>
    private void HandleJumpStateEvent(Enum state, int level)
    {
        var js = (PlayerStates.JumpState)state;
        if (js == PlayerStates.JumpState.Jump 
            && levelCrystalMap.TryGetValue(1, out var crystal))
        {
            crystal.transform.DOPunchScale(
                punch:    new Vector3(0.3f, 0.3f, 0.3f),
                duration: 0.4f,
                vibrato:  8
            );
        }
    }

    /// <summary>
    /// Optional: punch-scale feedback when player starts sprinting.
    /// </summary>
    private void HandleSprintStateEvent(Enum state, int level)
    {
        var ss = (PlayerStates.SprintState)state;
        if (ss == PlayerStates.SprintState.StartSprinting 
            && levelCrystalMap.TryGetValue(2, out var crystal))
        {
            crystal.transform.DOPunchScale(
                punch:    new Vector3(0.3f, 0.3f, 0.3f),
                duration: 0.3f,
                vibrato:  8
            );
        }
    }

    /// <summary>
    /// Optional: punch-scale feedback when player starts ground pound.
    /// </summary>
    private void HandleGroundPoundStateEvent(Enum state, int level)
    {
        var gp = (PlayerStates.GroundPoundState)state;
        if (gp == PlayerStates.GroundPoundState.StartGroundPound 
            && levelCrystalMap.TryGetValue(3, out var crystal))
        {
            crystal.transform.DOPunchScale(
                punch:    new Vector3(0.3f, 0.3f, 0.3f),
                duration: 0.4f,
                vibrato:  8
            );
        }
    }
}
