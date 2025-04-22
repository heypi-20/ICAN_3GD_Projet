using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Jauge_Energy : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private S_EnergyStorage energyStorage;
    [SerializeField] private Renderer targetRenderer;
    
    [Header("Curves & Durée")]
    [SerializeField] private AnimationCurve emissionCurve;
    [SerializeField] private AnimationCurve sharpnessCurve;
    [SerializeField] private AnimationCurve fillCurve;
    [SerializeField] private float duration = 1f;

    [Header("Valeurs visuelles")]
    [SerializeField] private float baseEmission = 1f;
    [SerializeField] private float maxEmission = 6f;
    [SerializeField] private float baseSharpness = 2f;
    [SerializeField] private float boostedSharpness = 10f;

    private static readonly int GlobalEmissionID = Shader.PropertyToID("_GlobalEmission");
    private static readonly int GradientSharpnessID = Shader.PropertyToID("_GradientSharpness");
    private static readonly int FillJaugeID = Shader.PropertyToID("_FillJauge");

    private MaterialPropertyBlock _mpb;
    private Coroutine _effectRoutine;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        energyStorage.OnLevelChange += OnLevelChangeReceived;
    }

    private void OnDestroy()
    {
        energyStorage.OnLevelChange -= OnLevelChangeReceived;
    }

    private void OnLevelChangeReceived(System.Enum state, int level)
    {
        if (state.ToString() == "LevelUp")
        {
            if (_effectRoutine != null) StopCoroutine(_effectRoutine);
            _effectRoutine = StartCoroutine(PlayPalierUpgradeEffect());
        }
    }

    private IEnumerator PlayPalierUpgradeEffect()
    {
        float currentFill = GetFillFromShader();
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;

            float emission = Mathf.Lerp(baseEmission, maxEmission, emissionCurve.Evaluate(t));
            float sharpness = Mathf.Lerp(baseSharpness, boostedSharpness, sharpnessCurve.Evaluate(t));
            float fill = Mathf.Lerp(0.5f, currentFill, fillCurve.Evaluate(t));

            targetRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(GlobalEmissionID, emission);
            _mpb.SetFloat(GradientSharpnessID, sharpness);
            _mpb.SetFloat(FillJaugeID, fill);
            targetRenderer.SetPropertyBlock(_mpb);

            timer += Time.deltaTime;
            yield return null;
        }

        // Remise à la valeur finale exacte
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(GlobalEmissionID, baseEmission);
        _mpb.SetFloat(GradientSharpnessID, baseSharpness);
        _mpb.SetFloat(FillJaugeID, GetFillFromShader());
        targetRenderer.SetPropertyBlock(_mpb);
    }

    private float GetFillFromShader()
    {
        return Mathf.Lerp(0f, 2f, 1f - (energyStorage.currentEnergy / energyStorage.maxEnergy));
    }
}
