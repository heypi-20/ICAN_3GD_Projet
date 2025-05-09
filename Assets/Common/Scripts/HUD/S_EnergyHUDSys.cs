using UnityEngine;
using UnityEngine.UI;

public class S_EnergyHUDSys : MonoBehaviour
{
    [Header("Energy Source")]
    public S_EnergyStorage energyStorage;

    [Header("Target Image (UI)")]
    public Image targetImage;

    [Header("Fill Range")]
    public Vector2 fillRange = new Vector2(0.2f, 0.8f);

    [Header("Lerp Settings")]
    [Tooltip("数值变化的平滑速度，越大越快贴近目标")]
    public float lerpSpeed = 5f;

    private string shaderProperty = "_Progress";
    private Material _runtimeMat;
    private float _currentMapped = 0f; // 当前显示的进度值

    void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage.material != null)
            _runtimeMat = targetImage.material = Instantiate(targetImage.material);
        else
            Debug.LogError("目标 Image 没有材质！");
    }

    void Update()
    {
        if (energyStorage == null || _runtimeMat == null) return;

        // 1) 计算目标值
        float curE = energyStorage.currentEnergy;
        int idx = energyStorage.currentLevelIndex;
        var levels = energyStorage.energyLevels;

        float startE = levels[idx].requiredEnergy;
        float endE = (idx < levels.Length - 1) 
            ? levels[idx + 1].requiredEnergy 
            : energyStorage.maxEnergy;

        float t = (endE > startE)
            ? Mathf.Clamp01((curE - startE) / (endE - startE))
            : 1f;

        float targetMapped = Mathf.Lerp(fillRange.x, fillRange.y, t);

        // 2) 平滑过渡到目标值
        _currentMapped = Mathf.Lerp(_currentMapped, targetMapped, Time.deltaTime * lerpSpeed);

        // 3) 写到材质
        _runtimeMat.SetFloat(shaderProperty, _currentMapped);
    }
}