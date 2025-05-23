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

    // Store each crystal's original localScale so we can restore it after shaking or for scale animations
    private Dictionary<int, Vector3> originalScales;

    // Current infinite shake tween for grace warning
    private Tween graceTween;
    private int graceLevel = -1;

    // Sprint usage tween
    private Tween sprintUsageTween;

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
        foreach (var kv in levelCrystalMap)
        {
            var rune = kv.Value.GetComponent<RuneDisplay>();
            if (rune == null) continue;

            if (kv.Key <= level)
            {
                // Show rune
                rune.SetState(RuneDisplay.SkillState.Activé);
            }
            else
            {
                // Hide rune with its own hide animation
                rune.SetState(RuneDisplay.SkillState.Désactivé);
            }
        }

        switch (lvlState)
        {
            case PlayerStates.LevelState.StartGrace:
                // Start infinite shake on the crystal for the current level
                StartGraceShake(level);
                break;

            case PlayerStates.LevelState.EndGrace:
                // Stop the shake and restore original scale
                KillGraceTween();
                break;

            default:
                // For LevelUp and LevelDown, ensure any grace shake is killed
                KillGraceTween();
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

        var orig = originalScales[level];
        graceTween = crystal.transform
            .DOPunchScale(orig * 0.2f, 0.5f, vibrato: 10, elasticity: 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
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
    /// Simple scale-up then scale-down effect when player jumps.
    /// </summary>
    private void HandleJumpStateEvent(Enum state, int level)
    {
        var js = (PlayerStates.JumpState)state;
        if (js == PlayerStates.JumpState.Jump 
            && levelCrystalMap.TryGetValue(1, out var crystal)
            && originalScales.TryGetValue(1, out var origScale))
        {
            crystal.transform
                .DOScale(origScale * 1.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }

    /// <summary>
    /// Scale-up then scale-down effect when player starts/stops sprinting.
    /// </summary>
    private void HandleSprintStateEvent(Enum state, int level)
    {
        var ss = (PlayerStates.SprintState)state;
        if (!levelCrystalMap.TryGetValue(2, out var crystal) 
            || !originalScales.TryGetValue(2, out var origScale))
            return;

        switch (ss)
        {
            case PlayerStates.SprintState.StartSprinting:
                // Begin looping scale animation until stop event
                sprintUsageTween = crystal.transform
                    .DOScale(origScale * 1.2f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetAutoKill(false);
                break;

            case PlayerStates.SprintState.StopSprinting:
                // Stop looping animation and restore scale
                if (sprintUsageTween != null)
                {
                    sprintUsageTween.Kill();
                    crystal.transform.localScale = origScale;
                    sprintUsageTween = null;
                }
                break;
        }
    }

    /// <summary>
    /// Simple scale-up then scale-down effect when player starts ground pound.
    /// </summary>
    private void HandleGroundPoundStateEvent(Enum state, int level)
    {
        var gp = (PlayerStates.GroundPoundState)state;
        if (gp == PlayerStates.GroundPoundState.StartGroundPound 
            && levelCrystalMap.TryGetValue(3, out var crystal)
            && originalScales.TryGetValue(3, out var origScale))
        {
            crystal.transform
                .DOScale(origScale * 1.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);
        }
    }
}