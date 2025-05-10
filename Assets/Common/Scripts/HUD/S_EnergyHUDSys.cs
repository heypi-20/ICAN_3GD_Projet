using UnityEngine;
using UnityEngine.UI;

public class S_EnergyHUDSys : MonoBehaviour
{
    [Header("Energy Source")]
    public S_EnergyStorage energyStorage;

    [Header("Target Image (UI)")]
    public Image targetImage;

    [Header("Fill Range")]
    [Tooltip("Map normalized energy [0–1] into this min/max shader range")]
    public Vector2 fillRange = new Vector2(0.2f, 0.8f);

    [Header("Lerp Settings")]
    [Tooltip("How fast the displayed value chases the actual value")]
    public float lerpSpeed = 5f;

    [Header("Level Colors")]
    [Tooltip("Must match number of levels in energyStorage.energyLevels")]
    public Color[] tintColors;
    public Color[] emissionColors;
    public Color[] noiseColors;

    [Header("Color Lerp Settings")]
    [Tooltip("How fast the shader colors blend when level changes")]
    public float colorLerpSpeed = 5f;

    [Header("Noise Speed Mapping")]
    [Tooltip("If diff < x, treat as x; if > y, treat as y")]
    public Vector2 diffRange = new Vector2(-50f, 50f);
    [Tooltip("Map clamped diff [x–y] into this min/max noise speed")]
    public Vector2 noiseSpeedRange = new Vector2(0.2f, 1.5f);
    [Tooltip("Seconds between diff samples (window size)")]
    public float diffSamplePeriod = 0.5f;
    [Tooltip("How fast noise speed chases target speed")]
    public float noiseSpeedLerpSpeed = 5f;

    private const string ProgressProp   = "_Progress";
    private const string TintProp       = "_TintColor";
    private const string EmissionProp   = "_EmissionColor";
    private const string NoiseColorProp = "_NoiseColor";
    private const string NoiseSpeedProp = "_NoiseSpeed";

    private Material runtimeMaterial;
    private float displayedValue;
    private float lastEnergySample;
    private float diffTimer;
    private float currentNoiseSpeed;

    // for color lerp
    private Color currentTint;
    private Color currentEmission;
    private Color currentNoiseColor;

    void Awake()
    {
        InitializeMaterialInstance();
        InitializeColors();

        // prime sample window
        lastEnergySample = energyStorage != null ? energyStorage.currentEnergy : 0f;
        diffTimer = 0f;
        // get initial noise speed from shader
        currentNoiseSpeed = runtimeMaterial.GetFloat(NoiseSpeedProp);
    }

    void Update()
    {
        if (energyStorage == null || runtimeMaterial == null) return;

        // 1) update fill progress
        float targetValue = ComputeTargetMappedValue();
        displayedValue = Mathf.Lerp(displayedValue, targetValue, Time.deltaTime * lerpSpeed);
        runtimeMaterial.SetFloat(ProgressProp, displayedValue);

        // 2) update colors
        UpdateColorTransition();

        // 3) update noise speed via diff
        UpdateNoiseSpeedByDiff();

        // 4) advance the sample window
        diffTimer += Time.deltaTime;
        if (diffTimer >= diffSamplePeriod)
        {
            lastEnergySample = energyStorage.currentEnergy;
            diffTimer -= diffSamplePeriod;
        }
    }

    private void InitializeMaterialInstance()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
        if (targetImage.material == null)
        {
            Debug.LogError("UI Image has no material to instantiate.");
            return;
        }
        runtimeMaterial      = Instantiate(targetImage.material);
        targetImage.material = runtimeMaterial;
    }

    private void InitializeColors()
    {
        int idx = energyStorage != null
            ? Mathf.Clamp(energyStorage.currentLevelIndex, 0, tintColors.Length - 1)
            : 0;
        currentTint       = tintColors[idx];
        currentEmission   = emissionColors[idx];
        currentNoiseColor = noiseColors[idx];

        runtimeMaterial.SetColor(TintProp,       currentTint);
        runtimeMaterial.SetColor(EmissionProp,   currentEmission);
        runtimeMaterial.SetColor(NoiseColorProp, currentNoiseColor);
    }

    private float ComputeTargetMappedValue()
    {
        float cur = energyStorage.currentEnergy;
        int idx   = energyStorage.currentLevelIndex;
        var lvls  = energyStorage.energyLevels;

        float start = lvls[idx].requiredEnergy;
        float end   = (idx < lvls.Length - 1)
            ? lvls[idx + 1].requiredEnergy
            : energyStorage.maxEnergy;
        float norm  = end > start 
            ? Mathf.Clamp01((cur - start) / (end - start)) 
            : 1f;
        return Mathf.Lerp(fillRange.x, fillRange.y, norm);
    }

    private void UpdateColorTransition()
    {
        int idx = Mathf.Clamp(energyStorage.currentLevelIndex, 0, tintColors.Length - 1);
        Color t = tintColors[idx],
              e = emissionColors[idx],
              n = noiseColors[idx];
        currentTint       = Color.Lerp(currentTint,       t, Time.deltaTime * colorLerpSpeed);
        currentEmission   = Color.Lerp(currentEmission,   e, Time.deltaTime * colorLerpSpeed);
        currentNoiseColor = Color.Lerp(currentNoiseColor, n, Time.deltaTime * colorLerpSpeed);

        runtimeMaterial.SetColor(TintProp,       currentTint);
        runtimeMaterial.SetColor(EmissionProp,   currentEmission);
        runtimeMaterial.SetColor(NoiseColorProp, currentNoiseColor);
    }

    /// <summary>
    /// Compute diff over the last sample window, clamp into diffRange,
    /// map into noiseSpeedRange, then lerp toward it at noiseSpeedLerpSpeed.
    /// </summary>
    private void UpdateNoiseSpeedByDiff()
    {
        float current = energyStorage.currentEnergy;
        float diff = current - lastEnergySample;

        // clamp diff
        float clamped = Mathf.Clamp(diff, diffRange.x, diffRange.y);
        // normalize 0–1
        float tNorm = (clamped - diffRange.x) / (diffRange.y - diffRange.x);
        // map into noise speed
        float targetNoise = Mathf.Lerp(noiseSpeedRange.x, noiseSpeedRange.y, tNorm);

        // smooth toward targetNoise
        currentNoiseSpeed = Mathf.Lerp(currentNoiseSpeed, targetNoise, Time.deltaTime * noiseSpeedLerpSpeed);
        runtimeMaterial.SetFloat(NoiseSpeedProp, currentNoiseSpeed);
    }
}
