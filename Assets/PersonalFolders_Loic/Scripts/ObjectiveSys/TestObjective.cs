using System;
using UnityEngine;

public class TestObjective : MonoBehaviour
{
    private S_ObjectiveManager objManager;

    private Objective killObjective;
    private Objective killCuby;
    
    private S_ObjectiveDisplay objectiveDisplay;
    
    private void Start()
    {
        objManager = new S_ObjectiveManager();
        killObjective = new Objective("KillEnemy", "Killed : {0}/{1}", 10);
        killCuby = new Objective("KillEnemy", "Killed Cuby : {0}/{1}", 10);
        objManager.AddObjective(killObjective);

        objectiveDisplay = GetComponent<S_ObjectiveDisplay>();
        objectiveDisplay.Init(killObjective);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            killObjective.AddProgress(1);
            killCuby.AddProgress(1);
        }
    }
}

