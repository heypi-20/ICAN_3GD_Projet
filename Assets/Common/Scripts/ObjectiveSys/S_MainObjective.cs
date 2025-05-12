// S_MainObjective.cs
using UnityEngine;

public class S_MainObjective : MonoBehaviour
{
    [Header("Objective Properties")]
    public ObjectiveParams objectiveParams;
    [SerializeField]
    private S_ObjectiveDisplay objectiveDisplay;

    private S_ObjectiveManager objectiveManager;
    private Objective killObjective;

    [Header("Lissage d'affichage")]
    [Tooltip("Vitesse de lissage pour le compteur d'objectif")]
    public float objectiveLerpSpeed = 5f;

    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le texte de progression")]
    public S_DotweenPlayer dotweenPlayer;
    [Tooltip("Temps minimal (s) entre deux déclenchements")]
    public float tweenCooldown = 0.1f;

    private float displayedValue;
    private float lastValue;
    private float lastTweenTime = -Mathf.Infinity;

    private void Start()
    {
        objectiveManager = GetComponent<S_ObjectiveSystem>().ObjectiveManager;
        killObjective = new Objective(
            objectiveParams.eventTrigger,
            objectiveParams.eventText,
            objectiveParams.currentValue,
            objectiveParams.maxValue
        );
        objectiveManager.AddObjective(killObjective);

        if (objectiveDisplay == null)
        {
            Debug.LogError("No Objective Display assigned!");
            enabled = false;
            return;
        }
        objectiveDisplay.Init(killObjective);

        // Valeurs de départ pour le lerp
        displayedValue = killObjective.CurrentValue;
        lastValue = displayedValue;

        EnemyBase.OnEnemyKilled += AddProgress;
    }

    private void Update()
    {
        if (killObjective == null || objectiveDisplay == null)
            return;

        // —— Lissage de l'affichage ——
        float target = killObjective.CurrentValue;
        displayedValue = Mathf.Lerp(displayedValue, target, Time.deltaTime * objectiveLerpSpeed);
        int shownValue = Mathf.RoundToInt(displayedValue);
        objectiveDisplay.UpdateProgress(shownValue);

        // —— Pop DOTween quand la vraie valeur diminue ——
        if (target < lastValue
            && Time.time - lastTweenTime >= tweenCooldown
            && dotweenPlayer != null)
        {
            dotweenPlayer.Play();
            lastTweenTime = Time.time;
        }

        lastValue = target;
    }

    private void AddProgress(EnemyType enemyType)
    {
        // On décrémente l'objectif ici ; l'affichage sera interpolé dans Update()
        killObjective.AddProgressNegative(1);
    }
}