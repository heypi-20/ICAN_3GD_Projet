using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Jauge_Energy : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private S_EnergyStorage energyStorage;
    [SerializeField] private Renderer[] renderers; // 0 = gain, 1 = perte

    [Header("Shader Settings")]
    private static readonly int FillJaugeID = Shader.PropertyToID("_FillJauge");
    private static readonly int EndFillColorAmountID = Shader.PropertyToID("_EndFillColorAmount");
    private static readonly int EndIntensityID = Shader.PropertyToID("_EndIntensity");

    [Header("Énergie dynamique (bord vert/rouge)")]
    [SerializeField] private float maxGainPerSecond = 100f;
    [SerializeField] private float maxWidth = 0.4f;
    [SerializeField] private float minIntensity = 0.5f;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float smoothTime = 0.5f;

    private float lastEnergy = 0f;

    // Valeurs brutes
    private float rawGain = 0f;
    private float rawLoss = 0f;

    // Valeurs lissées
    private float smoothedGain = 0f;
    private float smoothedLoss = 0f;

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        lastEnergy = energyStorage.currentEnergy;
    }

    private void Update()
    {
        if (energyStorage == null || renderers.Length == 0 || energyStorage.energyLevels.Length == 0)
            return;

        float current = energyStorage.currentEnergy;
        float delta = current - lastEnergy;

        rawGain = delta > 0 ? delta / Time.deltaTime : 0f;
        rawLoss = delta < 0 ? -delta / Time.deltaTime : 0f;

        // Lissage
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
        float fillValue = Mathf.Lerp(0f, 2f, 1f - ratio);

        // Préparation des valeurs dynamiques
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
                // Jauge gain
                _mpb.SetFloat(EndFillColorAmountID, gainAmount);
                _mpb.SetFloat(EndIntensityID, gainIntensity);
            }
            else if (i == 1)
            {
                // Jauge perte
                _mpb.SetFloat(EndFillColorAmountID, lossAmount);
                _mpb.SetFloat(EndIntensityID, lossIntensity);
            }

            rend.SetPropertyBlock(_mpb);
        }
    }
}
