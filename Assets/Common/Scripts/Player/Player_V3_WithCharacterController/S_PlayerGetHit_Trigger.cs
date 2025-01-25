using Cinemachine.PostFX;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class S_PlayerGetHit_Trigger : MonoBehaviour
{
    private S_EnergyStorage _energyStorage;

    [Header("Camera Settings")]
    public CinemachineVolumeSettings _volumeSettings; // Référence au composant Volume de la caméra

    private Vignette _vignette; // Composant Vignette

    [Header("Vignette Effect Settings")]
    public float vignetteTargetIntensity = 0.4f; // Intensité cible de la vignette
    public float animationDuration = 0.5f; // Durée de l'animation (aller-retour)
    public Ease animationEase = Ease.InOutSine; // Type d'animation (Ease)

    private void Start()
    {
        // Initialisation de la référence au stockage d'énergie
        _energyStorage = GetComponent<S_EnergyStorage>();

        // Vérifier si le Volume est configuré et contient un effet Vignette
        if (_volumeSettings == null)    
        {
            Debug.LogWarning("No Volume Settings Found");
        }
        // Vérifier si le Volume Profile contient un effet Vignette
        if (_volumeSettings.m_Profile != null && _volumeSettings.m_Profile.TryGet<Vignette>(out _vignette))
        {
            Debug.Log("Vignette effect found in the Volume Profile.");
            _vignette.intensity.value = 0f;
        }
        else
        {
            Debug.LogError("No Vignette effect found in the Volume Profile!");
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_energyStorage == null || _vignette == null) return;

        // Réduire l'énergie du joueur en fonction des dégâts de l'ennemi
        var enemy = other.gameObject.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            _energyStorage.RemoveEnergy(enemy.enemyDamage);
        }

        // Animer l'effet de vignette
        AnimateVignetteEffect();
    }

    private void AnimateVignetteEffect()
    {
        // Assurez-vous que l'animation actuelle est arrêtée pour éviter des conflits
        DOTween.Kill(_vignette);

        // Faire varier l'intensité de la vignette avec une animation en deux étapes
        DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, vignetteTargetIntensity, animationDuration / 2)
            .SetEase(animationEase)
            .OnComplete(() =>
            {
                DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0, animationDuration / 2)
                    .SetEase(animationEase);
            });
    }
}
