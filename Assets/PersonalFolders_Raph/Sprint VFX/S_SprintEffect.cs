using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

[RequireComponent(typeof(S_BasicSprint_Module))]
[RequireComponent(typeof(VisualEffect))]
public class S_SprintEffect : MonoBehaviour
{
    private S_BasicSprint_Module sprintModule;
    private VisualEffect vfx;
    private float _initialSpawnRate;
    private float _initialSpeed;
    public float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        sprintModule = GetComponent<S_BasicSprint_Module>();
        vfx = GetComponent<VisualEffect>();

        // On lit tes valeurs de base une fois pour toutes
        _initialSpawnRate = vfx.GetFloat("SpawnRate");
        _initialSpeed = vfx.GetFloat("Speed");

        // On part de 0 si on veut qu'il n'y ait aucun effet au menu par exemple
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);

        sprintModule.OnSprintStateChange += HandleSprintState;
    }

    private void OnDestroy()
    {
        sprintModule.OnSprintStateChange -= HandleSprintState;
    }

    private void HandleSprintState(System.Enum stateEnum, int level)
    {
        switch (stateEnum)
        {
            case PlayerStates.SprintState.StartSprinting:
                StartFade(0f, 1f);
                break;
            case PlayerStates.SprintState.StopSprinting:
                StartFade(1f, 0f);
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
}