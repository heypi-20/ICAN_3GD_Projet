using System;
using UnityEngine;

public class TestObjective : MonoBehaviour
{
    [Header("Objective Properties")]
    public ObjectiveParams objectiveParams;
    [SerializeField]
    private S_ObjectiveDisplay objectiveDisplay;

    private S_ObjectiveManager ObjectiveManager;
    private Objective killObjective;
    
    private void Start()
    {
        ObjectiveManager = GetComponent<S_ObjectiveSystem>().ObjectiveManager;
        
        killObjective = new Objective(objectiveParams.eventTrigger, objectiveParams.eventText, objectiveParams.currentValue, objectiveParams.maxValue);
        ObjectiveManager.AddObjective(killObjective);
        
        if (objectiveDisplay == null) {
            Debug.LogError("No Objective Display assigned!");
        }
        objectiveDisplay.Init(killObjective);
        
        EnemyBase.OnEnemyKilled += AddProgress;
    }

    private void AddProgress(EnemyType enemyType)
    {
        killObjective.AddProgress(1);
    }
}
