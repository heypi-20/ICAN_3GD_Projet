using UnityEngine;
using System.Collections;

public class S_Jauge_Energy : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private S_EnergyStorage energyStorage;
    [SerializeField] private Renderer jaugeRenderer;

    [Header("Shader Settings")]
    private static readonly int FillJaugeID = Shader.PropertyToID("_FillJauge");
    private static readonly int EndFillColorAmountID = Shader.PropertyToID("_EndFillColorAmount");
    private static readonly int EndIntensityID = Shader.PropertyToID("_EndIntensity");
    private static readonly int ColorTintID = Shader.PropertyToID("_ColorTint");
    private static readonly int GlobalEmissionID = Shader.PropertyToID("_GlobalEmission");
    private static readonly int GradientSharpnessID = Shader.PropertyToID("_GradientSharpness");
    private static readonly int PulseSpeedID = Shader.PropertyToID("_PulseSpeed");
    private static readonly int WaveSpeedID = Shader.PropertyToID("_WaveSpeed");
    private static readonly int WaveFrequencyID = Shader.PropertyToID("_WaveFrequency");
    private static readonly int AmplitudeID = Shader.PropertyToID("_Amplitude");
    private static readonly int NoiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
    
    private Color targetTint;
    private float currentTintAmount;
    private float currentIntensity;
    private Color currentColor;

    [Header("Jauge Fill")]
    [SerializeField] private float fillMin = 0.5f;
    [SerializeField] private float fillMax = 1.5f;

    [Header("Énergie dynamique")]
    [SerializeField] private float maxDeltaPerSecond = 100f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float maxFillAmount = 0.4f;
    [SerializeField] private float refreshInterval = 0.5f;

    [Header("Couleurs personnalisées")]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    [Header("Boost Palier Effect")]
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private float returnDuration = 0.3f;
    [SerializeField] private AnimationCurve fillCurve;
    [SerializeField] private AnimationCurve emissionCurve;
    [SerializeField] private AnimationCurve sharpnessCurve;
    [SerializeField] private AnimationCurve pulseCurve;
    [SerializeField] private AnimationCurve waveSpeedCurve;
    [SerializeField] private AnimationCurve waveFreqCurve;
    [SerializeField] private AnimationCurve amplitudeCurve;

    [SerializeField] private float boostedEmission = 6f;
    [SerializeField] private float boostedSharpness = 8f;
    [SerializeField] private float boostedPulseSpeed = 20f;
    [SerializeField] private float boostedWaveSpeed = 5f;
    [SerializeField] private float boostedWaveFrequency = 2f;
    [SerializeField] private float boostedAmplitude = 2f;

    [SerializeField] private float baseEmission = 1f;
    [SerializeField] private float baseSharpness = 2f;
    [SerializeField] private float basePulseSpeed = 0.5f;
    [SerializeField] private float baseWaveSpeed = 1f;
    [SerializeField] private float baseWaveFrequency = 1f;
    [SerializeField] private float baseAmplitude = 1f;
    
    [Header("Effet Grace Period")]
    [SerializeField] private float graceEnterDuration = 5f;
    [SerializeField] private float graceExitDuration = 0.5f;
    [SerializeField] private float graceEmission = 4f;
    [SerializeField] private float gracePulseSpeed = 15f;
    [SerializeField] private float graceWaveSpeed = 3f;
    [SerializeField] private float graceWaveFrequency = 1.5f;
    [SerializeField] private float graceAmplitude = 2f;
    [SerializeField] private float graceNoiseIntensity = 1.5f;
    [SerializeField] private Color graceColor = Color.red;
    [SerializeField] private AnimationCurve graceEnterCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve graceExitCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Coroutine _graceEffectRoutine;
    private float lastEnergy;
    private static readonly int EndFillColorID = Shader.PropertyToID("_EndFillColor");
    private float lastRefreshTime;
    private float smoothedDelta;
    private MaterialPropertyBlock _mpb;
    private Coroutine _palierEffectRoutine;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        lastEnergy = energyStorage.currentEnergy;
        lastRefreshTime = Time.time;
        energyStorage.OnLevelChange += OnLevelChangeReceived;
    }

    private void OnDestroy()
    {
        energyStorage.OnLevelChange -= OnLevelChangeReceived;
    }

    private void Update()
    {
        if (energyStorage == null || jaugeRenderer == null || energyStorage.energyLevels.Length == 0)
            return;

        UpdateFillBar();

        if (Time.time - lastRefreshTime >= refreshInterval)
        {
            UpdateBalanceVisual();
            lastRefreshTime = Time.time;
        }
        
        // Smooth visual feedback
        currentColor = Color.Lerp(currentColor, targetTint, Time.deltaTime * 5f);
        float smoothAmount = Mathf.Lerp(jaugeRenderer.sharedMaterial.GetFloat(EndFillColorAmountID), currentTintAmount, Time.deltaTime * 5f);
        float smoothIntensity = Mathf.Lerp(jaugeRenderer.sharedMaterial.GetFloat(EndIntensityID), currentIntensity, Time.deltaTime * 5f);

        jaugeRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EndFillColorID, currentColor);
        _mpb.SetFloat(EndFillColorAmountID, smoothAmount);
        _mpb.SetFloat(EndIntensityID, smoothIntensity);
        jaugeRenderer.SetPropertyBlock(_mpb);
    }

    private void UpdateFillBar()
    {
        float current = energyStorage.currentEnergy;
        int index = energyStorage.currentLevelIndex;

        float min = energyStorage.energyLevels[index].requiredEnergy;
        float max = (index + 1 < energyStorage.energyLevels.Length) ? energyStorage.energyLevels[index + 1].requiredEnergy : energyStorage.maxEnergy;

        float ratio = Mathf.InverseLerp(min, max, current);
        float fillValue = Mathf.Lerp(fillMin, fillMax, 1f - ratio);

        jaugeRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FillJaugeID, fillValue);
        jaugeRenderer.SetPropertyBlock(_mpb);
    }

    private void UpdateBalanceVisual()
    {
        targetTint = smoothedDelta >= 0 ? positiveColor : negativeColor;
        float clamped = Mathf.Clamp01(Mathf.Abs(smoothedDelta) / maxDeltaPerSecond);
        currentTintAmount = clamped * maxFillAmount;
        currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, clamped);
        float current = energyStorage.currentEnergy;
        float delta = (current - lastEnergy) / refreshInterval;
        lastEnergy = current;

        smoothedDelta = Mathf.Lerp(smoothedDelta, delta, 0.5f);
        
        float amount = clamped * maxFillAmount;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, clamped);
        Color tint = smoothedDelta >= 0 ? positiveColor : negativeColor;

        jaugeRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(EndFillColorAmountID, amount);
        _mpb.SetFloat(EndIntensityID, intensity);
        _mpb.SetColor(ColorTintID, tint);
        _mpb.SetColor(EndFillColorID, tint);
        jaugeRenderer.SetPropertyBlock(_mpb);
    }
    

    private void OnLevelChangeReceived(System.Enum state, int level)
    {
        if (state.ToString() == "LevelUp")
        {
            if (_palierEffectRoutine != null) StopCoroutine(_palierEffectRoutine);
            _palierEffectRoutine = StartCoroutine(PlayPalierEffect());
        }
        else if (state.ToString() == "StartGrace")
        {
            if (_graceEffectRoutine != null) StopCoroutine(_graceEffectRoutine);
            _graceEffectRoutine = StartCoroutine(GraceEffect(true));
        }
        else if (state.ToString() == "EndGrace")
        {
            if (_graceEffectRoutine != null) StopCoroutine(_graceEffectRoutine);
            _graceEffectRoutine = StartCoroutine(GraceEffect(false));
        }
    }

    private IEnumerator PlayPalierEffect()
    {
        float current = energyStorage.currentEnergy;
        int index = energyStorage.currentLevelIndex;

        float min = energyStorage.energyLevels[index].requiredEnergy;
        float max = (index + 1 < energyStorage.energyLevels.Length) ? energyStorage.energyLevels[index + 1].requiredEnergy : energyStorage.maxEnergy;
        float trueFill = Mathf.Lerp(fillMin, fillMax, 1f - Mathf.InverseLerp(min, max, current));

        float timer = 0f;
        while (timer < effectDuration)
        {
            float t = timer / effectDuration;

            jaugeRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(GlobalEmissionID, Mathf.Lerp(baseEmission, boostedEmission, emissionCurve.Evaluate(t)));
            _mpb.SetFloat(GradientSharpnessID, Mathf.Lerp(baseSharpness, boostedSharpness, sharpnessCurve.Evaluate(t)));
            _mpb.SetFloat(FillJaugeID, Mathf.Lerp(fillMin, trueFill, fillCurve.Evaluate(t)));
            _mpb.SetFloat(PulseSpeedID, Mathf.Lerp(basePulseSpeed, boostedPulseSpeed, pulseCurve.Evaluate(t)));
            _mpb.SetFloat(WaveSpeedID, Mathf.Lerp(baseWaveSpeed, boostedWaveSpeed, waveSpeedCurve.Evaluate(t)));
            _mpb.SetFloat(WaveFrequencyID, Mathf.Lerp(baseWaveFrequency, boostedWaveFrequency, waveFreqCurve.Evaluate(t)));
            _mpb.SetFloat(AmplitudeID, Mathf.Lerp(baseAmplitude, boostedAmplitude, amplitudeCurve.Evaluate(t)));
            jaugeRenderer.SetPropertyBlock(_mpb);

            timer += Time.deltaTime;
            yield return null;
        }

        float returnTimer = 0f;
        while (returnTimer < returnDuration)
        {
            float t = returnTimer / returnDuration;

            jaugeRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat(GlobalEmissionID, Mathf.Lerp(boostedEmission, baseEmission, t));
            _mpb.SetFloat(GradientSharpnessID, Mathf.Lerp(boostedSharpness, baseSharpness, t));
            _mpb.SetFloat(FillJaugeID, Mathf.Lerp(trueFill, trueFill, t));
            _mpb.SetFloat(PulseSpeedID, Mathf.Lerp(boostedPulseSpeed, basePulseSpeed, t));
            _mpb.SetFloat(WaveSpeedID, Mathf.Lerp(boostedWaveSpeed, baseWaveSpeed, t));
            _mpb.SetFloat(WaveFrequencyID, Mathf.Lerp(boostedWaveFrequency, baseWaveFrequency, t));
            _mpb.SetFloat(AmplitudeID, Mathf.Lerp(boostedAmplitude, baseAmplitude, t));
            jaugeRenderer.SetPropertyBlock(_mpb);

            returnTimer += Time.deltaTime;
            yield return null;
        }
    }
    
    private IEnumerator GraceEffect(bool entering)
{
    float duration = entering ? graceEnterDuration : graceExitDuration;
    AnimationCurve curve = entering ? graceEnterCurve : graceExitCurve;
    float timer = 0f;

    float startEmission     = entering ? baseEmission     : graceEmission;
    float endEmission       = entering ? graceEmission     : baseEmission;
    float startPulse        = entering ? basePulseSpeed   : gracePulseSpeed;
    float endPulse          = entering ? gracePulseSpeed   : basePulseSpeed;
    float startWave         = entering ? baseWaveSpeed    : graceWaveSpeed;
    float endWave           = entering ? graceWaveSpeed    : baseWaveSpeed;
    float startFreq         = entering ? baseWaveFrequency: graceWaveFrequency;
    float endFreq           = entering ? graceWaveFrequency: baseWaveFrequency;
    float startAmp          = entering ? baseAmplitude    : graceAmplitude;
    float endAmp            = entering ? graceAmplitude    : baseAmplitude;
    float startNoise        = entering ? 1f               : graceNoiseIntensity;
    float endNoise          = entering ? graceNoiseIntensity: 1f;
    Color startColor        = entering ? Color.white       : graceColor;
    Color endColor          = entering ? graceColor        : Color.white;

    while (timer < duration)
    {
        float t = timer / duration;
        float eval = curve.Evaluate(t);

        jaugeRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(GlobalEmissionID, Mathf.Lerp(startEmission, endEmission, eval));
        _mpb.SetFloat(PulseSpeedID, Mathf.Lerp(startPulse, endPulse, eval));
        _mpb.SetFloat(WaveSpeedID, Mathf.Lerp(startWave, endWave, eval));
        _mpb.SetFloat(WaveFrequencyID, Mathf.Lerp(startFreq, endFreq, eval));
        _mpb.SetFloat(AmplitudeID, Mathf.Lerp(startAmp, endAmp, eval));
        _mpb.SetFloat(NoiseIntensityID, Mathf.Lerp(startNoise, endNoise, eval));
        _mpb.SetColor(ColorTintID, Color.Lerp(startColor, endColor, eval));
        jaugeRenderer.SetPropertyBlock(_mpb);

        timer += Time.deltaTime;
        yield return null;
    }

    // Final apply (clean)
    jaugeRenderer.GetPropertyBlock(_mpb);
    _mpb.SetFloat(GlobalEmissionID, endEmission);
    _mpb.SetFloat(PulseSpeedID, endPulse);
    _mpb.SetFloat(WaveSpeedID, endWave);
    _mpb.SetFloat(WaveFrequencyID, endFreq);
    _mpb.SetFloat(AmplitudeID, endAmp);
    _mpb.SetFloat(NoiseIntensityID, endNoise);
    _mpb.SetColor(ColorTintID, endColor);
    jaugeRenderer.SetPropertyBlock(_mpb);
    }
}
