using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_ObjectivePanel : MonoBehaviour
{
    [SerializeField]
    private S_ObjectiveDisplay _objectiveDisplayPrefab;

    [SerializeField]
    private Transform _objectiveDisplayParent;

    private readonly List<S_ObjectiveDisplay> _listDisplay = new();
    private S_ObjectiveManager ObjectiveManager;

    private void Start()
    {
        ObjectiveManager = FindObjectOfType<S_ObjectiveSystem>().ObjectiveManager;
        
        if (ObjectiveManager == null)
            return;
        
        foreach(Objective objective in ObjectiveManager.Objectives) {
            AddObjective(objective);
        }
        ObjectiveManager.OnObjectiveAdded += AddObjective;
    }

    private void AddObjective(Objective obj) {
        var display = Instantiate(_objectiveDisplayPrefab, _objectiveDisplayParent);
        display.Init(obj);
        _listDisplay.Add(display);
    }

    public void Reset() {
        for (var i = _listDisplay.Count - 1; i >= 0; i--) {
            Destroy(_listDisplay[i].gameObject);
        }
        _listDisplay.Clear();
    }
}

