using TMPro;
using UnityEngine;
using DG.Tweening;

public class S_ObjectiveDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _objectiveText;

    private Objective _objective;
    private int _displayedValue;

    [Tooltip("Durée du tween en secondes pour la valeur")]
    [SerializeField]
    private float tweenDuration = 0.5f;

    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le KillText (pop/scale/etc.)")]
    public S_DotweenPlayer dotweenPlayer;
    [Tooltip("Temps minimal (s) entre deux déclenchements de l'animation")]
    public float tweenCooldown = 0.1f;

    // pour gérer le cooldown de l'animation
    private float _lastTweenTime = -Mathf.Infinity;
    private int _lastTweenValue;

    private void Awake()
    {
        _objectiveText = GetComponent<TextMeshProUGUI>();
    }

    public void Init(Objective objective)
    {
        _objective = objective;

        // Valeur initiale
        _displayedValue = objective.CurrentValue;
        _lastTweenValue = _displayedValue;
        _objectiveText.text = _objective.GetStatusText();

        // Abonnement aux événements
        objective.OnValueChange += OnObjectiveValueChange;
        objective.OnComplete += OnObjectiveComplete;
    }

    private void OnObjectiveComplete()
    {
        // Barrer le texte lorsque l'objectif est terminé
        _objectiveText.text = $"<s>{_objectiveText.text}</s>";
    }

    private void OnObjectiveValueChange()
    {
        int newValue = _objective.CurrentValue;

        // Tween numérique de la valeur affichée vers la nouvelle valeur
        DOTween.Kill(_objectiveText);
        DOTween.To(() => _displayedValue,
                   x => {
                       _displayedValue = x;
                       _objectiveText.text = $"{_displayedValue}/{_objective.MaxValue}";
                   },
                   newValue,
                   tweenDuration)
               .SetTarget(_objectiveText);

        // Pop DOTweenPlayer avec cooldown
        if (dotweenPlayer != null
            && newValue != _lastTweenValue
            && Time.time - _lastTweenTime >= tweenCooldown)
        {
            dotweenPlayer.Play();
            _lastTweenTime = Time.time;
            _lastTweenValue = newValue;
        }
    }
}