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

    [HideInInspector] public float gameChrono;
    private bool once = true;
    
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

    private void Update()
    {
        gameChrono += Time.deltaTime;

        if (killObjective.IsComplete && once) {
            Debug.Log("objective is complete");
            int min = Mathf.FloorToInt(gameChrono / 60f);
            int sec = Mathf.FloorToInt(gameChrono - min % 60f);
            string timeFormat = $"{sec:00}:{min:00}";
            objectiveDisplay.GetGameTime(timeFormat);
            Debug.Log("Game Time : " + timeFormat);
            once = false;
        }
    }

    private void AddProgress(EnemyType enemyType)
    {
        killObjective.AddProgressNegative(1);
        dotweenPlayer.Play();
    }
}