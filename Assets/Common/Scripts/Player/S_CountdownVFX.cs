using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(S_EnergyStorage))]
public class S_CountdownVFX : MonoBehaviour
{
    [Header("Countdown Settings")]
    public float countdownDuration = 10f; // Durée totale du compte à rebours
    public TextMeshProUGUI countdownText; // Texte affichant le compte à rebours
    public Image countdownImage; // Image dont la transparence change pendant le compte à rebours

    private S_EnergyStorage _energyStorage; // Référence à S_EnergyStorage pour surveiller l'énergie actuelle
    private float _currentCountdownTime; // Temps restant du compte à rebours
    private bool _isCountingDown; // Indique si le compte à rebours est en cours
    private Tween _textShakeTween; // Animation DOTween pour le tremblement du texte
    private Tween _imageFadeTween; // Animation DOTween pour la transparence de l'image

    private void Start()
    {
        this.enabled = false;
        // Initialisation du module
        _energyStorage = GetComponent<S_EnergyStorage>();
        ResetCountdown(); // Réinitialisation des paramètres
    }

    private void Update()
    {
        // Vérifie et surveille l'état de l'énergie à chaque frame
        MonitorEnergyState();
    }

    private void MonitorEnergyState()
    {
        // Si l'énergie devient négative et que le compte à rebours n'a pas démarré, démarre le compte à rebours
        if (_energyStorage.currentEnergy < 0f && !_isCountingDown)
        {
            StartCountdown();
        }
        // Si l'énergie redevient positive, arrête le compte à rebours
        else if (_energyStorage.currentEnergy >= 0f && _isCountingDown)
        {
            StopCountdown();
        }

        // Si le compte à rebours est en cours, met à jour son affichage
        if (_isCountingDown)
        {
            UpdateCountdown();
        }
    }

    private void StartCountdown()
    {
        // Démarrage du compte à rebours
        _isCountingDown = true;
        _currentCountdownTime = countdownDuration;

        // Animation tremblante pour le texte (DOTween)
        _textShakeTween = countdownText.rectTransform
            .DOShakePosition(0.5f, new Vector3(5, 5, 0), 20, 90, false, true)
            .SetLoops(-1, LoopType.Yoyo);

        // Animation de transparence pour l'image (DOTween)
        _imageFadeTween = countdownImage.DOFade(1f, countdownDuration).From(0f).SetEase(Ease.Linear);

        // Activation des objets visuels
        countdownText.gameObject.SetActive(true);
        countdownImage.gameObject.SetActive(true);
    }

    private void UpdateCountdown()
    {
        // Mise à jour du temps restant
        _currentCountdownTime -= Time.deltaTime;

        // Met à jour le texte avec le temps restant arrondi
        countdownText.text = Mathf.CeilToInt(_currentCountdownTime).ToString();

        // Si le temps atteint 0, termine le compte à rebours
        if (_currentCountdownTime <= 0f)
        {
            EndCountdown();
        }
    }

    private void StopCountdown()
    {
        // Arrêt du compte à rebours
        _isCountingDown = false;

        // Réinitialisation des valeurs
        ResetCountdown();

        // Désactivation des objets visuels
        countdownText.gameObject.SetActive(false);
        countdownImage.gameObject.SetActive(false);

        // Arrêt des animations DOTween
        _textShakeTween?.Kill();
        _imageFadeTween?.Kill();
        countdownImage.color = new Color(countdownImage.color.r, countdownImage.color.g, countdownImage.color.b, 0f);
        GetComponent<S_CountdownVFX>().enabled = false;
    }

    private void EndCountdown()
    {
        // Chargement de la scène actuelle pour simuler un redémarrage
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetCountdown()
    {
        // Réinitialisation des paramètres de compte à rebours
        _currentCountdownTime = countdownDuration;
        countdownText.text = countdownDuration.ToString();

        // Réinitialisation de la transparence de l'image
        countdownImage.color = new Color(countdownImage.color.r, countdownImage.color.g, countdownImage.color.b, 0f);
    }
}
