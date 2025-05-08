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
    public CinemachineVirtualCamera cinemachineCamera;
    public float fovTransitionTime = 0.2f;
    [Tooltip("FOV à activer lors du sprint")]
    public float sprintFOV = 80f;
    private float normalFOV;

    [Header("Speedlines Follow")]
    [Tooltip("Référence à ton script S_GunFollowCamera")]
    public S_GunFollowCamera gunFollowCamera;
    private float defaultDistance;

    // Indique si les speedlines sont actives (on a sprinté et pas encore arrêté)
    private bool isSpeedlineActive = false;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        _initialSpawnRate = vfx.GetFloat("SpawnRate");
        _initialSpeed = vfx.GetFloat("Speed");
        // Départ à 0 pour pas d'effet au menu
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
    }

    private void Start()
    {
        var obs = S_PlayerStateObserver.Instance;
        obs.OnSprintStateEvent += HandleSprintState;

        // On mémorise les valeurs de base
        if (cinemachineCamera != null)
            normalFOV = cinemachineCamera.m_Lens.FieldOfView;
        if (gunFollowCamera != null)
            defaultDistance = gunFollowCamera.distanceFromCamera;
    }

    private void HandleSprintState(System.Enum stateEnum, int level)
    {
        switch (stateEnum)
        {
            case PlayerStates.SprintState.StartSprinting:
                isSpeedlineActive = true;
                StartFade(0f, 1f);
                TweenCameraFOV(sprintFOV);
                break;

            case PlayerStates.SprintState.StopSprinting:
                isSpeedlineActive = false;
                StartFade(1f, 0f);
                TweenCameraFOV(normalFOV);
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

            vfx.SetFloat("SpawnRate", intensity * _initialSpawnRate);
            vfx.SetFloat("Speed", intensity * _initialSpeed);
            yield return null;
        }
        vfx.SetFloat("SpawnRate", to * _initialSpawnRate);
        vfx.SetFloat("Speed", to * _initialSpeed);
        fadeRoutine = null;
    }

    private void TweenCameraFOV(float targetFOV)
    {
        if (cinemachineCamera == null) return;
        DOTween.Kill(cinemachineCamera);
        DOTween.To(
            () => cinemachineCamera.m_Lens.FieldOfView,
            x => cinemachineCamera.m_Lens.FieldOfView = x,
            targetFOV,
            fovTransitionTime
        ).SetEase(Ease.OutQuad);
    }

    // À chaque frame, tant que le sprint est actif, on recalcule la distance
    private void LateUpdate()
    {
        if (!isSpeedlineActive || gunFollowCamera == null || cinemachineCamera == null)
            return;

        float currentFOV = cinemachineCamera.m_Lens.FieldOfView;
        AdjustSpeedlinesDistance(currentFOV);
    }

    /// <summary>
    /// Compense la distance du gunFollowCamera selon le FOV
    /// pour que l'effet garde le même "apparent size".
    /// </summary>
    private void AdjustSpeedlinesDistance(float targetFOV)
    {
        float halfNormalRad = normalFOV * Mathf.Deg2Rad * 0.5f;
        float halfTargetRad = targetFOV * Mathf.Deg2Rad * 0.5f;
        float ratio = Mathf.Tan(halfNormalRad) / Mathf.Tan(halfTargetRad);

        gunFollowCamera.distanceFromCamera = defaultDistance * ratio;
    }

    private void OnDisable()
    {
        if (S_PlayerStateObserver.Instance != null)
            S_PlayerStateObserver.Instance.OnSprintStateEvent -= HandleSprintState;
    }
}