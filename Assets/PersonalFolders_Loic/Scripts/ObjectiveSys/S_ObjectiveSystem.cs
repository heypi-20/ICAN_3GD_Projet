using System;
using UnityEngine;

public class S_ObjectiveSystem : MonoBehaviour
{
    public S_ObjectiveManager ObjectiveManager { get; private set; }
    
    private void Awake()
    {
        ObjectiveManager = new S_ObjectiveManager();

        Objective killObjective = new Objective("KillEnemy", "Killed : {0}/{1}", 10);
        ObjectiveManager.AddObjective(killObjective);
    }   
}

