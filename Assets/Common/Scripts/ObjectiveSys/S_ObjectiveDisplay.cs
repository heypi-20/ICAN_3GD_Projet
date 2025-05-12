using System;
using TMPro;
using UnityEngine;

public class S_ObjectiveDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _objectiveText;

    private Objective _objective;

    private void Awake()
    {
        _objectiveText = GetComponent<TextMeshProUGUI>();
    }

    public void Init(Objective objective)
    {
        _objective = objective;
        _objectiveText.text = objective.GetStatusText();
        objective.OnValueChange += OnObjectiveValueChange;
        objective.OnComplete += OnObjectiveComplete;
    }

    private void OnObjectiveComplete()
    {
        _objectiveText.text = $"<s>{_objective.GetStatusText()}</s>";
    }

    private void OnObjectiveValueChange()
    {
        _objectiveText.text = _objective.GetStatusText();
    }
}