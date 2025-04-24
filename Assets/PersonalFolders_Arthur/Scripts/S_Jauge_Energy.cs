using UnityEngine;
using System.Collections;

public class S_Jauge_Energy : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private S_EnergyStorage energyStorage;
    [SerializeField] private Renderer[] renderers; // 0 = gain, 1 = perte

    [Header("Shader Settings")]
    private static readonly int FillJaugeID = Shader.PropertyToID("_FillJauge");
    private static readonly int EndFillColorAmountID = Shader.PropertyToID("_EndFillColorAmount");
    private static readonly int EndIntensityID = Shader.PropertyToID("_EndIntensity");
    private static readonly int GlobalEmissionID = Shader.PropertyToID("_GlobalEmission");
    private static readonly int GradientSharpnessID = Shader.PropertyToID("_GradientSharpness");

    [Header("Énergie dynamique (bord vert/rouge)")]
    [SerializeField] private float maxGainPerSecond = 100f;
    [SerializeField] private float maxWidth = 0.4f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float smoothTime = 0.5f;

    [Header("Effet visuel de gain de palier")]
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private float returnDuration = 0.3f;
    [SerializeField] private AnimationCurve fillCurve;
    [SerializeField] private AnimationCurve emissionCurve;
    [SerializeField] private AnimationCurve sharpnessCurve;
    [SerializeField] private float boostedEmission = 6f;
    [SerializeField] private float boostedSharpness = 8f;
    [SerializeField] private float baseEmission = 1f;
    [SerializeField] private float baseSharpness = 2f;

    private float lastEnergy = 0f;
    private float smoothedGain = 0f;
    private float smoothedLoss = 0f;
    private MaterialPropertyBlock _mpb;
    private Coroutine _palierEffectRoutine;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        lastEnergy = energyStorage.currentEnergy;
        energyStorage.OnLevelChange += OnLevelChangeReceived;
    }

    private void OnDestroy()
    {
        energyStorage.OnLevelChange -= OnLevelChangeReceived;
    }

    private void Update()
    {
        if (energyStorage == null || renderers.Length == 0 || energyStorage.energyLevels.Length == 0)
            return;

        float current = energyStorage.currentEnergy;
        float delta = current - lastEnergy;

        float rawGain = delta > 0 ? delta / Time.deltaTime : 0f;
        float rawLoss = delta < 0 ? -delta / Time.deltaTime : 0f;

        smoothedGain = Mathf.Lerp(smoothedGain, rawGain, Time.deltaTime / smoothTime);
        smoothedLoss = Mathf.Lerp(smoothedLoss, rawLoss, Time.deltaTime / smoothTime);

        lastEnergy = current;

        UpdateShaderFill();
    }

    private void UpdateShaderFill()
    {
        float current = energyStorage.currentEnergy;
        int index = energyStorage.currentLevelIndex;

        float min = energyStorage.energyLevels[index].requiredEnergy;
        float max = (index + 1 < energyStorage.energyLevels.Length)
            ? energyStorage.energyLevels[index + 1].requiredEnergy
            : energyStorage.maxEnergy;

        float ratio = Mathf.InverseLerp(min, max, current);
        float fillValue = Mathf.Lerp(0.5f, 1.5f, 1f - ratio);

        float gainClamped = Mathf.Clamp01(smoothedGain / maxGainPerSecond);
        float lossClamped = Mathf.Clamp01(smoothedLoss / maxGainPerSecond);

        float gainAmount = gainClamped * maxWidth;
        float gainIntensity = Mathf.Lerp(minIntensity, maxIntensity, gainClamped);

        float lossAmount = lossClamped * maxWidth;
        float lossIntensity = Mathf.Lerp(minIntensity, maxIntensity, lossClamped);

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null) continue;

            rend.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FillJaugeID, fillValue);

            if (i == 0)
            {
                _mpb.SetFloat(EndFillColorAmountID, gainAmount);
                _mpb.SetFloat(EndIntensityID, gainIntensity);
            }
            else if (i == 1)
            {
                _mpb.SetFloat(EndFillColorAmountID, lossAmount);
                _mpb.SetFloat(EndIntensityID, lossIntensity);
            }

            rend.SetPropertyBlock(_mpb);
        }
    }

    private void OnLevelChangeReceived(System.Enum state, int level)
    {
        if (state.ToString() == "LevelUp")
        {
            if (_palierEffectRoutine != null) StopCoroutine(_palierEffectRoutine);
            _palierEffectRoutine = StartCoroutine(PlayPalierEffect());
        }
    }

    private IEnumerator PlayPalierEffect()
    {
        float current = energyStorage.currentEnergy;
        int index = energyStorage.currentLevelIndex;

        float min = energyStorage.energyLevels[index].requiredEnergy;
        float max = (index + 1 < energyStorage.energyLevels.Length)
            ? energyStorage.energyLevels[index + 1].requiredEnergy
            : energyStorage.maxEnergy;

        float trueFill = Mathf.Lerp(0.5f, 1.5f, 1f - Mathf.InverseLerp(min, max, current));

        float timer = 0f;
        while (timer < effectDuration)
        {
            float t = timer / effectDuration;

            float emission = Mathf.Lerp(baseEmission, boostedEmission, emissionCurve.Evaluate(t));
            float sharpness = Mathf.Lerp(baseSharpness, boostedSharpness, sharpnessCurve.Evaluate(t));
            float fill = Mathf.Lerp(0.5f, trueFill, fillCurve.Evaluate(t));

            foreach (Renderer rend in renderers)
            {
                if (rend == null) continue;

                rend.GetPropertyBlock(_mpb);
                _mpb.SetFloat(GlobalEmissionID, emission);
                _mpb.SetFloat(GradientSharpnessID, sharpness);
                _mpb.SetFloat(FillJaugeID, fill);
                rend.SetPropertyBlock(_mpb);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Retour smooth aux vraies valeurs
        float returnTimer = 0f;
        float startEmission = boostedEmission;
        float startSharpness = boostedSharpness;
        float startFill = 0.5f;

        while (returnTimer < returnDuration)
        {
            float t = returnTimer / returnDuration;

            float emission = Mathf.Lerp(startEmission, baseEmission, t);
            float sharpness = Mathf.Lerp(startSharpness, baseSharpness, t);
            float fill = Mathf.Lerp(startFill, trueFill, t);

            foreach (Renderer rend in renderers)
            {
                if (rend == null) continue;

                rend.GetPropertyBlock(_mpb);
                _mpb.SetFloat(GlobalEmissionID, emission);
                _mpb.SetFloat(GradientSharpnessID, sharpness);
                _mpb.SetFloat(FillJaugeID, fill);
                rend.SetPropertyBlock(_mpb);
            }

            returnTimer += Time.deltaTime;
            yield return null;
        }
        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            rend.GetPropertyBlock(_mpb);
            _mpb.SetFloat(GlobalEmissionID, baseEmission);
            _mpb.SetFloat(GradientSharpnessID, baseSharpness);
            rend.SetPropertyBlock(_mpb);
        }
    }
}
