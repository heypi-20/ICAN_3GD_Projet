using System;
using UnityEngine;

public class S_MainObjective : MonoBehaviour
{
    [Header("Objective Properties")]
    public ObjectiveParams objectiveParams;
    [SerializeField]
    private S_ObjectiveDisplay objectiveDisplay;

    private S_ObjectiveManager ObjectiveManager;
    private Objective killObjective;

    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le KillText")]
    public S_DotweenPlayer dotweenPlayer;

    private void Start()
    {
        ObjectiveManager = GetComponent<S_ObjectiveSystem>().ObjectiveManager;

        killObjective = new Objective(objectiveParams.eventTrigger, objectiveParams.eventText, objectiveParams.currentValue, objectiveParams.maxValue);
        ObjectiveManager.AddObjective(killObjective);

        if (objectiveDisplay == null)
        {
            Debug.LogError("No Objective Display assigned!");
        }
        objectiveDisplay.Init(killObjective);

        EnemyBase.OnEnemyKilled += AddProgress;
    }

    private void AddProgress(EnemyType enemyType)
    {
        killObjective.AddProgressNegative(1);
        dotweenPlayer.Play();
    }
}