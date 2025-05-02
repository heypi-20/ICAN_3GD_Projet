using System.Collections.Generic;
using UnityEngine;

public class S_ObjectivePanel : MonoBehaviour
{
    [SerializeField]
    private S_ObjectiveDisplay _objectiveDisplayPrefab;

    [SerializeField]
    private Transform _objectiveDisplayParent;

    private readonly List<S_ObjectiveDisplay> _listDisplay = new();

    void Start() {
        // Assumes you have a GameManager Singleton with the ObjectiveManager
        // foreach (var objective in GameManager.Instance.Objectives.Objectives) {
        //     AddObjective(objective);
        // }
        //
        // GameManager.Instance.Objectives.OnObjectiveAdded += AddObjective;
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

