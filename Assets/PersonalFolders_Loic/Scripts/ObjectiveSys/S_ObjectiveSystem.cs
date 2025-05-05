using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectiveParams
{
    public string eventTrigger;
    public string eventText;
    public int maxValue;
}

public class S_ObjectiveSystem : MonoBehaviour
{
    public S_ObjectiveManager ObjectiveManager { get; private set; }
    
    private void Awake()
    {
        ObjectiveManager = new S_ObjectiveManager();

        // foreach(ObjectiveParam objectiveParam in objectives) {
        //     Objective objective = new Objective(objectiveParam.eventTrigger, String.Concat(objectiveParam.eventText, " {0}/{1}"), objectiveParam.maxValue);
        //     ObjectiveManager.AddObjective(objective);
        // }
    }   
}

