using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Cinemachine;
using DG.Tweening;

[RequireComponent(typeof(VisualEffect))]
public class S_SprintEffect : MonoBehaviour
{
    [Header("VFX Settings")]
    private VisualEffect vfx;
    private float _initialSpawnRate;
    private float _initialSpeed;
    public float fadeDuration = 0.5f;
    private Coroutine fadeRoutine;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera cinemachineCamera; // Caméra du joueur pour ajuster le FOV
    public float fovTransitionTime = 0.2f; // Durée de transition du FOV
    public float SprintFOV = 60f; // FOV sprint
    private float normalFOV = 60f; // FOV normal

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();

        // On lit tes valeurs de base une fois pour toutes
        _initialSpawnRate = vfx.GetFloat("SpawnRate");
        _initialSpeed = vfx.GetFloat("Speed");

        // On part de 0 si on veut qu'il n'y ait aucun effet au menu par exemple
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
    }

    private void Start()
    {
        S_PlayerStateObserver.Instance.OnSprintStateEvent += HandleSprintState;

        if (cinemachineCamera != null)
        {
            normalFOV = cinemachineCamera.m_Lens.FieldOfView;
        }
    }

    private void HandleSprintState(System.Enum stateEnum, int level)
    {
        switch (stateEnum)
        {
            case PlayerStates.SprintState.StartSprinting:
                StartFade(0f, 1f);
                UpdateCameraFOV(SprintFOV);
                break;
            case PlayerStates.SprintState.StopSprinting:
                StartFade(1f, 0f);
                UpdateCameraFOV(normalFOV);
                break;
        }
    }

    private void StartFade(float from, float to)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(from, to));
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / fadeDuration);
            float intensity = Mathf.Lerp(from, to, norm);

            // On fait 0 → ta valeur de base
            vfx.SetFloat("SpawnRate", intensity * _initialSpawnRate);
            vfx.SetFloat("Speed", intensity * _initialSpeed);

            yield return null;
        }
        // Assure-toi d'être pile sur la valeur cible
        vfx.SetFloat("SpawnRate", to * _initialSpawnRate);
        vfx.SetFloat("Speed", to * _initialSpeed);

        fadeRoutine = null;
    }

    private void UpdateCameraFOV(float targetFOV)
    {
        if (cinemachineCamera != null)
        {
            DOTween.Kill(cinemachineCamera); // Arrête les animations FOV précédentes
            DOTween.To(
                () => cinemachineCamera.m_Lens.FieldOfView, // Getter pour le FOV actuel
                x => cinemachineCamera.m_Lens.FieldOfView = x, // Setter pour appliquer le nouveau FOV
                targetFOV, // Valeur cible
                fovTransitionTime // Durée de la transition
            ).SetEase(Ease.OutQuad);
        }
    }
}