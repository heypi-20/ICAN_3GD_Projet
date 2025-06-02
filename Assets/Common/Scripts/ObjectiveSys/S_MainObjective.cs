using System;
using UnityEngine;

public class S_MainObjective : MonoBehaviour
{
    [Header("Objective Properties")]
    public ObjectiveParams objectiveParams;
    [SerializeField]
    private S_ObjectiveDisplay objectiveDisplay;

    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le KillText")]
    public S_DotweenPlayer dotweenPlayer;
    
    private S_ObjectiveManager ObjectiveManager;
    private Objective killObjective;

    [Header("Read-Only Properties")]
    public int objectiveCurrentValue;
    
    private void Start()
    {
        ObjectiveManager = GetComponent<S_ObjectiveSystem>().ObjectiveManager;

        killObjective = new Objective(
            objectiveParams.eventTrigger,
            objectiveParams.eventText,
            objectiveParams.currentValue,
            objectiveParams.maxValue
            );
        killObjective.OnComplete += () =>
        {
            FindObjectOfType<BossPhaseActivator>().StartBossPhase();
        };

        ObjectiveManager.AddObjective(killObjective);

        if (objectiveDisplay == null)
        {
            Debug.LogError("No Objective Display assigned!");
        }
        objectiveDisplay.Init(killObjective);

        EnemyBase.OnEnemyKilled += AddProgress;
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyKilled -= AddProgress;
    }

    private void Update()
    {
        objectiveCurrentValue = killObjective.CurrentValue;
    }


    private void AddProgress(EnemyType enemyType)
    {
        if (killObjective.IsComplete)
            return;
        killObjective.AddProgressNegative(1);
        dotweenPlayer.Play();
    }
}