using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Jauge_Energy : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private S_EnergyStorage energyStorage;
    [SerializeField] private Renderer[] renderers; // Tu peux en mettre 1, 2 ou plus
    private static readonly int FillJaugeID = Shader.PropertyToID("_FillJauge");

    private MaterialPropertyBlock _mpb;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (energyStorage == null || renderers.Length == 0 || energyStorage.energyLevels.Length == 0)
            return;

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

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;
            rend.GetPropertyBlock(_mpb);
            _mpb.SetFloat(FillJaugeID, fillValue);
            rend.SetPropertyBlock(_mpb);
        }
    }
}
