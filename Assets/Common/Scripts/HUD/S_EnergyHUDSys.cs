using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  // DOTween

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

    [Header("Additional Level Image")]
    [Tooltip("Optional UI Image to tint per level and animate on energy increase")]
    public Image levelImage;
    [Tooltip("Must match number of levels in energyStorage.energyLevels")]
    public Color[] levelImageColors;
    [Tooltip("Scaling punch strength when energy increases")]
    public Vector3 punchScale = Vector3.one * 0.1f;
    [Tooltip("Duration of the punch animation")]
    public float punchDuration = 0.3f;

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

    // for shader color lerp
    private Color currentTint;
    private Color currentEmission;
    private Color currentNoiseColor;

    // for levelImage color lerp
    private Color currentImageColor;

    // for detecting energy increase
    private float lastEnergyValue;
    private Vector3 originalImageScale;
    private Tween punchTween;

    void Awake()
    {
        InitializeMaterialInstance();
        InitializeColors();

        // Record the original scale of levelImage
        if (levelImage != null)
            originalImageScale = levelImage.transform.localScale;

        // Prime sample window & energy-increase detector
        lastEnergySample  = energyStorage != null ? energyStorage.currentEnergy : 0f;
        lastEnergyValue   = lastEnergySample;
        diffTimer         = 0f;
        currentNoiseSpeed = runtimeMaterial.GetFloat(NoiseSpeedProp);
    }

    void Update()
    {
        if (energyStorage == null || runtimeMaterial == null) return;

        float curEnergy = energyStorage.currentEnergy;

        // —— Energy Increase Detection + Smooth Punch Animation —— 
        if (levelImage != null && curEnergy > lastEnergyValue)
        {
            // Kill previous punch to avoid overlap, reset scale
            if (punchTween != null && punchTween.IsActive())
                punchTween.Kill();
            levelImage.transform.localScale = originalImageScale;

            // Start new punch animation
            punchTween = levelImage.transform
                .DOPunchScale(punchScale, punchDuration, vibrato: 10, elasticity: 1f)
                .SetAutoKill(true);
        }
        lastEnergyValue = curEnergy;

        // —— 1) Update Fill Progress —— 
        float targetValue = ComputeTargetMappedValue();
        displayedValue = Mathf.Lerp(displayedValue, targetValue, Time.deltaTime * lerpSpeed);
        runtimeMaterial.SetFloat(ProgressProp, displayedValue);

        // —— 2) Update Color Transition —— 
        UpdateColorTransition();

        // —— 3) Update Noise Speed —— 
        UpdateNoiseSpeedByDiff();

        // —— 4) Advance Sample Window —— 
        diffTimer += Time.deltaTime;
        if (diffTimer >= diffSamplePeriod)
        {
            lastEnergySample = curEnergy;
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

        if (levelImage != null && levelImageColors != null && levelImageColors.Length > idx)
        {
            currentImageColor = levelImageColors[idx];
            levelImage.color  = currentImageColor;
        }
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

        if (levelImage != null && levelImageColors != null && levelImageColors.Length > idx)
        {
            Color targetImgCol = levelImageColors[idx];
            currentImageColor  = Color.Lerp(currentImageColor, targetImgCol, Time.deltaTime * colorLerpSpeed);
            levelImage.color   = currentImageColor;
        }
    }

    private void UpdateNoiseSpeedByDiff()
    {
        float current = energyStorage.currentEnergy;
        float diff = current - lastEnergySample;

        float clamped = Mathf.Clamp(diff, diffRange.x, diffRange.y);
        float tNorm = (clamped - diffRange.x) / (diffRange.y - diffRange.x);
        float targetNoise = Mathf.Lerp(noiseSpeedRange.x, noiseSpeedRange.y, tNorm);

        currentNoiseSpeed = Mathf.Lerp(currentNoiseSpeed, targetNoise, Time.deltaTime * noiseSpeedLerpSpeed);
        runtimeMaterial.SetFloat(NoiseSpeedProp, currentNoiseSpeed);
    }
}
