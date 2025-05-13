using DG.Tweening;
using TMPro;
using UnityEngine;

public class S_ObjectiveDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _objectiveText;
    [Tooltip("Duree du tween en secondes pour la valeur")]
    [SerializeField]
    private float tweenDuration = 0.5f;
    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le KillText (pop/scale/etc.)")]
    public S_DotweenPlayer dotweenPlayer;
    [Tooltip("Temps minimal (s) entre deux d�clenchements de l'animation")]
    public float tweenCooldown = 0.1f;
    
    private Objective _objective;
    [HideInInspector] public string gameTime;
    
    private int _displayedValue;

    // pour g�rer le cooldown de l'animation
    private float _lastTweenTime = -Mathf.Infinity;
    private int _lastTweenValue;

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

    public void GetGameTime(string formatedTime)
    {
        gameTime = formatedTime;
    }
    
    private void OnObjectiveComplete()
    {
        _objectiveText.text = gameTime;
    }

    private void OnObjectiveValueChange()
    {
        int newValue = _objective.CurrentValue;
        
        // Tween num�rique de la valeur affich�e vers la nouvelle valeur
        DOTween.Kill(_objectiveText);
        DOTween.To(() => _displayedValue,
                   x => {
                       _displayedValue = x;
                       _objectiveText.text = _objective.GetStatusText();
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