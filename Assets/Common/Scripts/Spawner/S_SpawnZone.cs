using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_SpawnZone : MonoBehaviour
{
    // List of spawn point groups for different enemy types.
    public List<S_EnemySpawnPointGroup> enemySpawnPointGroups;
    // Radius of the spawn zone.
    public float radius;
    // Timer for weight loss.
    public float weightLossTimer;
    // Curve that defines how weight decreases over time.
    public AnimationCurve weightLossCurve;
    // Timer for weight gain.
    public float weightGainTimer;
    // Curve that defines how weight increases over time.
    public AnimationCurve weightGainCurve;
    
    // Current weight value used to control spawn behavior.
    public float weight = 0f;
    // Flag to indicate if this zone has already triggered another zone.
    [HideInInspector]
    public bool alreadyTriggerAnotherZone = false;
    
    // Gizmos settings for visual debugging in the editor.
    public bool drawGizmos;
    public Color gizmosColor;
    
    // References to running weight timers.
    private Coroutine weightLossCoroutine;
    private Coroutine weightGainCoroutine;
    // Flag to mark if the zone is waiting to be reactivated.
    [HideInInspector]
    public bool isWaitingToBeReactivated = false;

    private void Start()
    {
        // Get all S_EnemySpawnPointGroup components from child objects
        // and store them in the enemySpawnPointGroups list.
        enemySpawnPointGroups = new List<S_EnemySpawnPointGroup>(GetComponentsInChildren<S_EnemySpawnPointGroup>());
    }

    // Returns spawn points for the specified enemy type.
    public Vector3[] GetSpawnPointsByEnemyType(EnemyType type)
    {
        foreach (S_EnemySpawnPointGroup group in enemySpawnPointGroups)
        {
            if (group.enemyType == type)
            {
                return group.spawnPoints;
            }
        }
        return null;
    }

    // Called when the player triggers this zone.
    // Starts the weight loss timer, which gradually reduces the weight.
    public void StartWeightLossTimer()
    {
        // Stop any weight gain timer if it is running.
        if (weightGainCoroutine != null)
        {
            StopCoroutine(weightGainCoroutine);
            weightGainCoroutine = null;
        }
        // Do nothing if a weight loss timer is already running.
        if (weightLossCoroutine != null) return;

        // Set the weight to 1 and start the weight loss coroutine.
        weight = 1f;
        weightLossCoroutine = StartCoroutine(WeightLossCoroutine());
    }

    // Coroutine to gradually reduce the weight based on the weightLossCurve.
    private IEnumerator WeightLossCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < weightLossTimer)
        {
            elapsed += Time.deltaTime;
            // Normalize elapsed time to a 0-1 range.
            float normalizedTime = elapsed / weightLossTimer;
            // Update weight using the curve.
            weight = weightLossCurve.Evaluate(normalizedTime);
            yield return null;
        }
        // Ensure the final weight is set correctly.
        weight = weightLossCurve.Evaluate(1f);
        weightLossCoroutine = null;
    }
    
    // Called when the zone is triggered by another zone.
    // Starts the weight gain timer, which gradually increases the weight.
    public void StartWeightGainTimer()
    {
        // Do nothing if a weight gain timer is already running.
        if (weightGainCoroutine != null) return;

        // Set the weight to 0 and start the weight gain coroutine.
        weight = 0f;
        weightGainCoroutine = StartCoroutine(WeightGainCoroutine());
    }

    // Coroutine to gradually increase the weight based on the weightGainCurve.
    private IEnumerator WeightGainCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < weightGainTimer)
        {
            elapsed += Time.deltaTime;
            // Normalize elapsed time to a 0-1 range.
            float normalizedTime = elapsed / weightGainTimer;
            // Update weight using the curve.
            weight = weightGainCurve.Evaluate(normalizedTime);
            yield return null;
        }
        // Ensure the final weight is set correctly.
        weight = weightGainCurve.Evaluate(1f);
        weightGainCoroutine = null;
    }

    private float previousWeight = 0f;

    // Store the weight value after updates for later comparison.
    private void LateUpdate()
    {
        previousWeight = weight;
    }

    // Draws a sphere gizmo in the editor to visualize the spawn zone.
    public void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Determine if weight is increasing.
        bool isGrowing = weight > previousWeight;

        // Choose colors based on whether weight is growing.
        Color from = isGrowing ? gizmosColor : Color.green;
        Color to   = isGrowing ? Color.green : gizmosColor;

        // Lerp between the two colors based on weight.
        Gizmos.color = Color.Lerp(from, to, isGrowing ? weight : 1f - weight);
        // Draw a sphere at the spawn zone's position with the specified radius.
        Gizmos.DrawSphere(transform.position, radius);
    }
}
