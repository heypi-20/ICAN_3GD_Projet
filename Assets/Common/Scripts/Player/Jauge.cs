using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jauge : MonoBehaviour
{
    public S_EnergyStorage EnergyStore;
    public List<MeshRenderer> GrowMesh;
    public List<Material> GrowMaterial = new List<Material>();

    public float palier1;
    public float palier2;
    public float palier3;
    public float palier4;


    public float refreshRate = 0.05f;

    public float _growth = 0.1f;
    public float _growDuration = 0.2f;

    [Range(0, 1)]
    public float _minGrow = 0f;

    [Range(0, 1)]
    public float _maxGrow = 1;

    public float _points = 0f; // Système de points en float
    public float maxPoints = 100f; // Points maximaux
    public float pointsToGrow = 10f; // Points nécessaires pour atteindre la croissance maximale

    private float _targetGrowValue; // Valeur de croissance ciblée
    private float _growValue;

    void Start()
    {
        //palier1 = 800;
        //palier2 = EnergyStore.energyLevels[2].requiredEnergy - EnergyStore.energyLevels[1].requiredEnergy;
        //palier3 = EnergyStore.energyLevels[3].requiredEnergy - EnergyStore.energyLevels[2].requiredEnergy;
        //palier4 = EnergyStore.energyLevels[3].requiredEnergy;

        for (int i = 0; i < GrowMesh.Count; i++)
        {
            for (int j = 0; j < GrowMesh[i].materials.Length; j++)
            {
                if (GrowMesh[i].materials[j].HasProperty("_Grow"))
                {
                    GrowMesh[i].materials[j].SetFloat("_Grow", _minGrow);
                    GrowMaterial.Add(GrowMesh[i].materials[j]);
                }
            }
        }
        UpdateTargetGrowValue();
    }

    private void Update()
    {
        if(EnergyStore.currentLevelIndex == 0)
        {
            _points = Mathf.Clamp(EnergyStore.currentEnergy, 0f, maxPoints); // Met à jour les points avec le système d'énergie
            UpdateTargetGrowValue();
            pointsToGrow = EnergyStore.energyLevels[1].requiredEnergy;
        }
        if (EnergyStore.currentLevelIndex == 1)
        {
            _points = Mathf.Clamp(EnergyStore.currentEnergy, 0f, maxPoints) - EnergyStore.energyLevels[1].requiredEnergy; // Met à jour les points avec le système d'énergie
            UpdateTargetGrowValue();
            pointsToGrow = EnergyStore.energyLevels[2].requiredEnergy - EnergyStore.energyLevels[1].requiredEnergy;
        }
        if (EnergyStore.currentLevelIndex == 2)
        {
            _points = Mathf.Clamp(EnergyStore.currentEnergy, 0f, maxPoints) - EnergyStore.energyLevels[2].requiredEnergy; // Met à jour les points avec le système d'énergie
            UpdateTargetGrowValue();
            pointsToGrow = EnergyStore.energyLevels[3].requiredEnergy - EnergyStore.energyLevels[2].requiredEnergy;
        }
        if (EnergyStore.currentLevelIndex == 3)
        {
            _points = Mathf.Clamp(EnergyStore.currentEnergy, 0f, maxPoints) - EnergyStore.energyLevels[3].requiredEnergy; // Met à jour les points avec le système d'énergie
            UpdateTargetGrowValue();
            pointsToGrow = 1200f;
        }

        // Mise à jour continue de la croissance vers la cible
        for (int i = 0; i < GrowMaterial.Count; i++)
        {
            StartCoroutine(UpdateGrow(GrowMaterial[i]));
        }
    }

    IEnumerator UpdateGrow(Material mat)
    {
        while (Mathf.Abs(_growValue - _targetGrowValue) > 0.01f)
        {
            // Interpolation vers la valeur cible (_targetGrowValue)
            _growValue = Mathf.MoveTowards(_growValue, _targetGrowValue, refreshRate / _growDuration);
            mat.SetFloat("_Grow", _growValue);
            if(_growValue * 10 > 5)
            {
                mat.SetFloat("_Emmissive", _growValue * 10);
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    // Fonction pour ajouter des points
    public void AddPoints(float points)
    {
        _points = Mathf.Clamp(_points + points, 0f, maxPoints);
        UpdateTargetGrowValue();
    }

    // Fonction pour retirer des points
    public void RemovePoints(float points)
    {
        _points = Mathf.Clamp(_points - points, 0f, maxPoints);
        UpdateTargetGrowValue();
    }

    // Calculer la valeur de croissance cible en fonction des points
    private void UpdateTargetGrowValue()
    {
        // Mapper les points (float) sur la plage de croissance [_minGrow, _maxGrow]
        _targetGrowValue = Mathf.Lerp(_minGrow, _maxGrow, Mathf.Clamp01(_points / pointsToGrow));
    }
}
