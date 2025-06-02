using System;
using UnityEngine;

public class S_BossObjective : MonoBehaviour
{
    [Header("Objective Properties")]
    public ObjectiveParams objectiveParams;

    private S_ObjectiveManager ObjectiveManager;
    private Objective bossObjective;
    
    private bool bansheeBossKilled = false;
    private bool cubyBossKilled = false;
    private bool tpShooterBossKilled = false;
    [HideInInspector] public float gameChrono;


    private void Start()
    {
        ObjectiveManager = GetComponent<S_ObjectiveSystem>().ObjectiveManager;

        bossObjective = new Objective(
            objectiveParams.eventTrigger,
            objectiveParams.eventText,
            objectiveParams.currentValue,
            objectiveParams.maxValue
        );

        bossObjective.OnComplete += () =>
        {
            bossObjective.SetCompletionTime(gameChrono);
            FindObjectOfType<S_GameResultCalcul>().ShowResultScreen();
        };
        ObjectiveManager.AddObjective(bossObjective);
        EnemyBase.OnEnemyKilled += CheckBosses;
        
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyKilled -= CheckBosses;
    }

    private void Update()
    {
        if (!bossObjective.IsComplete) {
            gameChrono += Time.deltaTime;
        }
    }

    private void CheckBosses(EnemyType enemyType)
    {
        if (enemyType == EnemyType.unknown_Boss) {
            bossObjective.AddProgress(1);
            bansheeBossKilled = true;
        }
        if (enemyType == EnemyType.Cuby_Boss) {
            bossObjective.AddProgress(1);
            cubyBossKilled = true;
        }
        if (enemyType == EnemyType.TpShooter_Boss) {
            bossObjective.AddProgress(1);
            tpShooterBossKilled = true;
        }
    }
}

