using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_SpawnZone : MonoBehaviour
{
    [Header("Spawn Zone Settings")]
    public List<S_EnemySpawnPointGroup> enemySpawnPointGroups;
    public float radius;
    public float weightLossTimer;
    public AnimationCurve weightLossCurve;
    public float weightGainTimer;
    public AnimationCurve weightGainCurve;
    
    [Header("Use for debugging")]
    public float weight=0f;
    public bool alreadyTriggerAnotherZone = false;
    
    [Header("Gizmos Settings")]
    public bool drawGizmos;
    public Color gizmosColor;
    
    private Coroutine weightLossCoroutine;
    private Coroutine weightGainCoroutine;
    [HideInInspector]
    public bool isWaitingToBeReactivated = false;


    public Vector3[] GetSpawnPointsByEnemyType(EnemyType type)
    {
        foreach (S_EnemySpawnPointGroup Group in enemySpawnPointGroups)
        {
            if (Group.enemyType == type)
            {
                return Group.spawnPoints;
            }
        }
        return null;
    }

    // Called when player triggers this zone.
    public void StartWeightLossTimer()
    {
        // Stop any gain timer
        if (weightGainCoroutine != null)
        {
            StopCoroutine(weightGainCoroutine);
            weightGainCoroutine = null;
        }
        // Avoid duplicate trigger
        if (weightLossCoroutine != null) return;

        // Set initial weight to 1 and start decay
        weight = 1f;
        weightLossCoroutine = StartCoroutine(WeightLossCoroutine());
    }

    
    private IEnumerator WeightLossCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < weightLossTimer)
        {
            elapsed += Time.deltaTime;
            // Normalize the elapsed time to 0-1 range
            float normalizedTime = elapsed / weightLossTimer;
            // Evaluate the curve using normalized time
            weight = weightLossCurve.Evaluate(normalizedTime);
            yield return null;
        }
        // Ensure the final weight is set to the value at the end of the curve
        weight = weightLossCurve.Evaluate(1f);
        weightLossCoroutine = null;
    }
    
    // Called when the zone is triggered by another zone (chain trigger)
    public void StartWeightGainTimer()
    {
        // Avoid duplicate trigger
        if (weightGainCoroutine != null) return;

        // Set initial weight to 0 and start gain
        weight = 0f;
        weightGainCoroutine = StartCoroutine(WeightGainCoroutine());
    }

    private IEnumerator WeightGainCoroutine()
    {
        float elapsed = 0f;
        while (elapsed < weightGainTimer)
        {
            elapsed += Time.deltaTime;
            // Normalize the elapsed time to 0-1 range
            float normalizedTime = elapsed / weightGainTimer;
            // Evaluate the curve using normalized time
            weight = weightGainCurve.Evaluate(normalizedTime);
            yield return null;
        }
        // Ensure the final weight is set to the value at the end of the curve
        weight = weightGainCurve.Evaluate(1f);
        weightGainCoroutine = null;
        
    }


    private float previousWeight = 0f;

    private void LateUpdate()
    {
        previousWeight = weight;
    }

    public void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        bool isGrowing = weight > previousWeight;

        // A = gizmosColor, B = Color.green
        Color from = isGrowing ? gizmosColor : Color.green;
        Color to   = isGrowing ? Color.green : gizmosColor;

        Gizmos.color = Color.Lerp(from, to, isGrowing ? weight : 1f - weight);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
