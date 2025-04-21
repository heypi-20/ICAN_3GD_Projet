using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class S_PlayerHitTrigger : MonoBehaviour
{
    private S_EnergyStorage _energyStorage;

    [Header("Camera Settings")]
    public CinemachineVolumeSettings _volumeSettings; // Référence au composant Volume de la caméra
    private Vignette _vignette; // Composant Vignette

    [Header("Vignette Effect Settings")]
    public float vignetteTargetIntensity = 0.4f; // Intensité cible de la vignette
    public float animationDuration = 0.5f; // Durée de l'animation (aller-retour)
    public Ease animationEase = Ease.InOutSine; // Type d'animation (Ease)

    [Header("UI Settings")]
    public Image countdownImage; // Image dont la transparence change pendant le compte à rebours
    private Tween _imageFadeTween; // Animation DOTween pour la transparence de l'image
    private bool isDead = false; 

    private void Start()
    {
        _energyStorage = GetComponent<S_EnergyStorage>();

        if (_volumeSettings == null)    
        {
            Debug.LogWarning("No Volume Settings Found");
        }

        if (_volumeSettings.m_Profile != null && _volumeSettings.m_Profile.TryGet<Vignette>(out _vignette))
        {
            _vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("No Vignette effect found in the Volume Profile!");
        }
    }

    void Update()
    {
        LastHit();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_energyStorage == null || _vignette == null) return;

        var enemy = other.gameObject.GetComponent<EnemyBase>();
        var projectile = other.gameObject.GetComponent<S_EnemyProjectileDamage>();

        if (isDead && (enemy != null || projectile != null))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (enemy != null)
        {
            ReceiveDamage(enemy.enemyDamage);
        }
        else if (projectile != null)
        {
            AnimateVignetteEffect();
            _energyStorage.RemoveEnergy(projectile.damage);
        }
    }

    public void ReceiveDamage(float damage)
    {
        AnimateVignetteEffect();
        _energyStorage.RemoveEnergy(damage);

    }


    private void LastHit()
    {
        if (_energyStorage.currentEnergy <= 0f)
        {
            if (!isDead)
            {
                isDead = true;
                _imageFadeTween?.Kill();
                _imageFadeTween = countdownImage.DOFade(0.3f, 0.2f).From(0f).SetEase(Ease.Linear);
            }
        }
        else
        {
            isDead = false;
            countdownImage.color = new Color(countdownImage.color.r, countdownImage.color.g, countdownImage.color.b, 0f);
        }
    }

    private void AnimateVignetteEffect()
    {
        DOTween.Kill(_vignette);

        DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, vignetteTargetIntensity, animationDuration / 2)
            .SetEase(animationEase)
            .OnComplete(() =>
            {
                DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0, animationDuration / 2)
                    .SetEase(animationEase);
            });
    }
}
