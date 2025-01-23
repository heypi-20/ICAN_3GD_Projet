using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jauge : MonoBehaviour
{
    public S_EnergyStorage EnergyStore;
    public List<MeshRenderer> GrowTreeMesh;
    public List<Material> GrowTreeMaterial = new List<Material>();

    public float refreshRate = 0.05f;

    public float _growth = 0.1f;
    public float _growDuration = 0.2f;

    [Range(0, 1)]
    public float _minGrow = 0f;

    [Range(0, 1)]
    public float _maxGrow = 1;

    public float _points = 0f; // Système de points en float
    public float maxPoints = 9000f; // Points maximaux
    public float pointsToGrow = 800f; // Points nécessaires pour atteindre la croissance maximale

    private float _targetGrowValue; // Valeur de croissance ciblée
    private float _growValue;

    void Start()
    {
        for (int i = 0; i < GrowTreeMesh.Count; i++)
        {
            for (int j = 0; j < GrowTreeMesh[i].materials.Length; j++)
            {
                if (GrowTreeMesh[i].materials[j].HasProperty("_Grow"))
                {
                    GrowTreeMesh[i].materials[j].SetFloat("_Grow", _minGrow);
                    GrowTreeMaterial.Add(GrowTreeMesh[i].materials[j]);
                }
            }
        }
        UpdateTargetGrowValue();
    }

    private void Update()
    {
        _points = EnergyStore.currentEnergy;
        // Ajouter ou retirer des points pour tester (exemple)
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddPoints(50f); // Ajouter des points (float)
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            RemovePoints(50f); // Retirer des points (float)
        }

        // Mise à jour continue de la croissance vers la cible
        for (int i = 0; i < GrowTreeMaterial.Count; i++)
        {
            StartCoroutine(UpdateGrow(GrowTreeMaterial[i]));
        }
    }

    IEnumerator UpdateGrow(Material mat)
    {
        while (Mathf.Abs(_growValue - _targetGrowValue) > 0.01f)
        {
            // Interpolation vers la valeur cible (_targetGrowValue)
            _growValue = Mathf.MoveTowards(_growValue, _targetGrowValue, refreshRate / _growDuration);
            mat.SetFloat("_Grow", _growValue);

            yield return new WaitForSeconds(refreshRate);
        }
    }

    // Fonction pour ajouter des points (float)
    public void AddPoints(float points)
    {
        _points = Mathf.Clamp(_points + points, 0f, maxPoints);
        UpdateTargetGrowValue();
        Debug.Log("Points ajoutés : " + points + ". Total : " + _points);
    }

    // Fonction pour retirer des points (float)
    public void RemovePoints(float points)
    {
        _points = Mathf.Clamp(_points - points, 0f, maxPoints);
        UpdateTargetGrowValue();
        Debug.Log("Points retirés : " + points + ". Total : " + _points);
    }

    // Calculer la valeur de croissance cible en fonction des points
    private void UpdateTargetGrowValue()
    {
        // Assurez-vous que les points ne dépassent pas maxPoints
        _points = Mathf.Clamp(_points, 0f, maxPoints);

        // Mapper les points sur la plage de croissance (minGrow à maxGrow)
        if (_points <= pointsToGrow)
        {
            _targetGrowValue = Mathf.Lerp(_minGrow, _maxGrow, _points / pointsToGrow);
        }
        else
        {
            // Si les points dépassent pointsToGrow, restez à la taille maximale
            _targetGrowValue = _maxGrow;
        }

        Debug.Log($"Target Grow Value mise à jour : {_targetGrowValue}, Points : {_points}");
    }
}
