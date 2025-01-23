using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jauge : MonoBehaviour
{

    public List<MeshRenderer> GrowTreeMesh;
    public List<Material> GrowTreeMaterial = new List<Material>();

    public bool _canGrow = true;
    public bool _debugGrowValue;
    public bool _getDisable;

    public float _growth = 0.1f;
    public float _growDuration = 0.2f;

    [Range(0, 1)]
    public float _minGrow = 0f;

    [Range(0, 1)]
    public float _maxGrow = 1;

    private float _growValue;
    private bool _isGrown = false;

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
    }

    private void Update()
    {
        if (_debugGrowValue)
        {
            Debug.DrawRay(transform.position, Vector3.up * _growValue, Color.red);
        }
        for (int i = 0; i < GrowTreeMaterial.Count; i++)
        {
            if (_canGrow == true)
            {
                StartCoroutine(Growing(GrowTreeMaterial[i]));
            }
        }
    }


    IEnumerator Growing(Material mat)
    {
        //On Fait Grandir La Plante
        _canGrow = false;
        mat = GrowTreeMaterial[0];
        if (_isGrown == false)
        {
            float t = 0f;
            float startGrowValue = _growValue;
            while (t < _growDuration)
            {
                //va de 0f à duration
                t += Time.deltaTime;
                //R va de 0 à 1 
                float r = t / _growDuration;

                _growValue = Mathf.Lerp(startGrowValue, startGrowValue + _growth, r);

                mat.SetFloat("_Grow", _growValue);

                yield return null;
            }

            _growValue = startGrowValue + _growth;
            _canGrow = true;
        }

        if (_growValue >= _maxGrow)
        {
            _isGrown = true;
        }
    }
}
