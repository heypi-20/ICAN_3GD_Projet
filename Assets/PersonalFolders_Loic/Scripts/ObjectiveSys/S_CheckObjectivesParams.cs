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
        
        if (objectivesArr.Length == 0)
            return;

        objectives = new List<MonoBehaviour>(objectivesArr);
        if (objectives.Count > 2) {
            objectives.RemoveAt(0);
            objectives.RemoveAt(0);
        }
    }
    
    private void OnValidate()
    {
        if (objectives == null)
            return;
        foreach(MonoBehaviour objective in objectives) {
            FieldInfo[] fields = objective.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach(FieldInfo fieldInfo in fields) {
                if (fieldInfo.GetValue(objective).GetType() == typeof(ObjectiveParams)) {
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
            Debug.LogError("Event text format example : EventName : '{0}/{1}'");
        }
        if (objectiveParams.maxValue <= 0) {
            Debug.LogError("Max value must be greater than zero!");
        }
    }
}

