using System;
using UnityEngine;

public class TestObjective : MonoBehaviour
{
    private S_ObjectiveManager ObjectiveManager;
    private Objective killObjective;
    private S_ObjectiveDisplay objectiveDisplay;
    
    private void Start()
    {
        ObjectiveManager = FindObjectOfType<S_ObjectiveSystem>().ObjectiveManager;

        objectiveDisplay = GetComponent<S_ObjectiveDisplay>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            ObjectiveManager.AddProgress("KillEnemy", 1);
        }
    }
}

