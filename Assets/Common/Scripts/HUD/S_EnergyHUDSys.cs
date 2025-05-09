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

    private const string ShaderProperty = "_Progress";
    private Material runtimeMaterial;
    private float displayedValue = 0f;

    void Awake()
    {
        InitializeMaterialInstance();
    }

    void Update()
    {
        if (energyStorage == null || runtimeMaterial == null)
            return;

        float target = ComputeTargetMappedValue();
        SmoothToTarget(target);
        ApplyToShader(displayedValue);
    }

    /// <summary>
    /// Creates a unique material instance so we never modify the shared asset.
    /// </summary>
    private void InitializeMaterialInstance()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage.material == null)
        {
            Debug.LogError("UI Image has no material to instantiate.");
            return;
        }

        runtimeMaterial = Instantiate(targetImage.material);
        targetImage.material = runtimeMaterial;
    }

    /// <summary>
    /// Converts current energy into a mapped value in [fillRange.x, fillRange.y].
    /// </summary>
    private float ComputeTargetMappedValue()
    {
        float current = energyStorage.currentEnergy;
        int idx = energyStorage.currentLevelIndex;
        var levels = energyStorage.energyLevels;

        // determine the energy thresholds for this level and the next
        float startThreshold = levels[idx].requiredEnergy;
        float endThreshold = (idx < levels.Length - 1)
            ? levels[idx + 1].requiredEnergy
            : energyStorage.maxEnergy;

        // get a normalized [0–1] progress within this level
        float normalized = endThreshold > startThreshold
            ? Mathf.Clamp01((current - startThreshold) / (endThreshold - startThreshold))
            : 1f;

        // map that [0–1] into our custom shader range
        return Mathf.Lerp(fillRange.x, fillRange.y, normalized);
    }

    /// <summary>
    /// Smoothly advances the displayed value toward the target each frame.
    /// </summary>
    private void SmoothToTarget(float target)
    {
        displayedValue = Mathf.Lerp(displayedValue, target, Time.deltaTime * lerpSpeed);
    }

    /// <summary>
    /// Pushes the value into the material so the shader updates.
    /// </summary>
    private void ApplyToShader(float value)
    {
        runtimeMaterial.SetFloat(ShaderProperty, value);
    }
}
