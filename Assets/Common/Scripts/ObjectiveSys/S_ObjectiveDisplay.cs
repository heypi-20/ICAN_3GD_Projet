// S_ObjectiveDisplay.cs
using UnityEngine;
using TMPro;

public class S_ObjectiveDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _objectiveText;
    private Objective _objective;

    private void Awake()
    {
        if (_objectiveText == null)
            _objectiveText = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Initialise le display avec l'objectif donné.
    /// </summary>
    public void Init(Objective objective)
    {
        _objective = objective;
        _objectiveText.text = _objective.GetStatusText();
        // On ne souscrit qu'à la complétion pour le strikethrough
        _objective.OnComplete += OnObjectiveComplete;
    }

    private void OnObjectiveComplete()
    {
        _objectiveText.text = $"<s>{_objective.GetStatusText()}</s>";
    }

    /// <summary>
    /// Appelé depuis S_MainObjective pour mettre à jour l'affichage de façon lissée.
    /// </summary>
    public void UpdateProgress(int value)
    {
        // On récupère le préfixe (le texte avant ':') pour le conserver
        string full = _objective.GetStatusText();
        int idx = full.IndexOf(':');
        string label = idx >= 0 ? full.Substring(0, idx) : full;
        // On reconstruit le texte avec la valeur interpolée
        _objectiveText.text = $"{label}: {value}/{_objective.MaxValue}";
    }
}