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

    // Mapping from level index to its crystal GameObject
    private Dictionary<int, GameObject> levelCrystalMap;

    // Store the original local scale of each crystal for reset after animations
    private Dictionary<int, Vector3> originalScales;

    // Tween for the infinite shake during grace warning
    private Tween graceTween;
    private int graceLevel = -1;

    // Tween for sprint usage pulse
    private Tween sprintUsageTween;

    private void Awake()
    {
        // Initialize level-to-crystal mapping
        levelCrystalMap = new Dictionary<int, GameObject>
        {
            { 1, jumpCrystal },
            { 2, sprintCrystal },
            { 3, groundPoundCrystal }
        };

        // Record the original scale of each crystal
        originalScales = new Dictionary<int, Vector3>();
        foreach (var kv in levelCrystalMap)
        {
            if (kv.Value != null)
                originalScales[kv.Key] = kv.Value.transform.localScale;
        }
    }

    private void Start()
    {
        // Subscribe to player state events
        S_PlayerStateObserver.Instance.OnLevelUpStateEvent     += HandleLevelChangeEvent;
        S_PlayerStateObserver.Instance.OnJumpStateEvent        += HandleJumpStateEvent;
        S_PlayerStateObserver.Instance.OnSprintStateEvent      += HandleSprintStateEvent;
        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += HandleGroundPoundStateEvent;
    }

    private void OnDestroy()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            // Unsubscribe from all events
            S_PlayerStateObserver.Instance.OnLevelUpStateEvent     -= HandleLevelChangeEvent;
            S_PlayerStateObserver.Instance.OnJumpStateEvent        -= HandleJumpStateEvent;
            S_PlayerStateObserver.Instance.OnSprintStateEvent      -= HandleSprintStateEvent;
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent -= HandleGroundPoundStateEvent;
        }
    }

    /// <summary>
    /// Handles level-up, start/end grace, and level-down states.
    /// Adjusts each crystal's display state based on the current global level.
    /// </summary>
    private void HandleLevelChangeEvent(Enum state, int level)
    {
        var lvlState = (PlayerStates.LevelState)state;

        // Update each crystal's state according to the current level
        foreach (var kv in levelCrystalMap)
        {
            int unlockLevel = kv.Key;
            var crystal = kv.Value;
            var rune = crystal.GetComponent<RuneDisplay>();
            if (rune == null) continue;

            RuneDisplay.SkillState newState;

            if (level < unlockLevel)
            {
                // Not unlocked yet
                newState = RuneDisplay.SkillState.Désactivé;
            }
            else
            {
                // Compute tier relative to unlock level
                int tier;
                if (unlockLevel == 1)
                {
                    // Jump crystal waits one extra level before upgrading
                    tier = level - unlockLevel - 1;
                }
                else
                {
                    tier = level - unlockLevel;
                }

                if (tier <= 0)
                    newState = RuneDisplay.SkillState.Activé;
                else if (tier == 1)
                    newState = RuneDisplay.SkillState.Niveau2;
                else
                    newState = RuneDisplay.SkillState.Niveau3;
            }

            rune.SetState(newState);
        }

        // Handle grace shake start/end
        switch (lvlState)
        {
            case PlayerStates.LevelState.StartGrace:
                BeginGraceShake(level);
                break;
            case PlayerStates.LevelState.EndGrace:
                StopGraceShake();
                break;
            default:
                // On level up or down, also stop any lingering shake
                StopGraceShake();
                break;
        }
    }

    /// <summary>
    /// Begins an infinite shake animation on the crystal of the given level.
    /// </summary>
    private void BeginGraceShake(int level)
    {
        StopGraceShake();

        if (!levelCrystalMap.TryGetValue(level, out var crystal) || crystal == null)
            return;

        graceLevel = level;
        var originalScale = originalScales[level];
        graceTween = crystal.transform
            .DOPunchScale(originalScale * 0.2f, 0.5f, vibrato: 10, elasticity: 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetAutoKill(false);
    }

    /// <summary>
    /// Stops the grace shake and restores the crystal's original scale.
    /// </summary>
    private void StopGraceShake()
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
    /// Pulses the jump crystal on jump.
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
    /// Begins or stops a looping pulse on the sprint crystal.
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
                sprintUsageTween = crystal.transform
                    .DOScale(origScale * 1.2f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetAutoKill(false);
                break;
            case PlayerStates.SprintState.StopSprinting:
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
    /// Pulses the ground pound crystal on ground pound start.
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
