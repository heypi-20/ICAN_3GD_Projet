using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class S_CheckObjectivesParams : MonoBehaviour
{
    private List<MonoBehaviour> objectives;
    private void Start()
    {
        MonoBehaviour[] objectivesArr = GetComponents<MonoBehaviour>();
        objectives = new List<MonoBehaviour>(objectivesArr);
        objectives.RemoveAt(0);
        objectives.RemoveAt(0);
    }
    
    private void Update()
    {
        foreach(MonoBehaviour objective in objectives) {
            FieldInfo[] fields = objective.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach(FieldInfo fieldInfo in fields) {
                Debug.Log("Check fields : " + fieldInfo.GetValue(objective));
                if (fieldInfo.GetValue(objective).GetType() == typeof(ObjectiveParams)) {
                    Debug.Log("Get Objective Params");
                    ObjectiveParams objParams = (ObjectiveParams)fieldInfo.GetValue(objective);
                    CheckObjectiveParams(objParams);
                }
            }
        }
    }
    
    private void CheckObjectiveParams(ObjectiveParams objectiveParams)
    {
        if (objectiveParams.eventTrigger == string.Empty) {
            Debug.LogError("Event trigger cannot be empty!");
        }
        if (objectiveParams.eventText == string.Empty) {
            Debug.LogError("Event text cannot be empty and have a fix format : EventName : '{0}/{1}");
        }
        if (objectiveParams.maxValue <= 0) {
            Debug.LogError("Max value must be greater than zero!");
        }
    }
}

