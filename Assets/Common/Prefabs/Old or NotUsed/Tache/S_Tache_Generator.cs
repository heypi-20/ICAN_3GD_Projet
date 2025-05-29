using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Tache_Generator : MonoBehaviour
{
    [Header("Mat�riaux")]
    [Tooltip("Liste des mat�riaux possibles � appliquer.")]
    public Material[] materials;

    [Header("�chelle al�atoire")]
    [Tooltip("�chelle minimale (uniforme)")]
    public float minScale = 1f;
    [Tooltip("�chelle maximale (uniforme)")]
    public float maxScale = 2f;

    private void Start()
    {
        ApplyRandomMaterial();
        ApplyRandomRotation();
        ApplyRandomScale();
    }

    void ApplyRandomMaterial()
    {
        if (materials.Length == 0) return;

        Material chosenMaterial = materials[Random.Range(0, materials.Length)];
        Renderer renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            renderer.material = chosenMaterial;
        }
        else
        {
            // Si plusieurs MeshRenderers (enfants), appliquer � tous
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material = chosenMaterial;
            }
        }
    }

    void ApplyRandomRotation()
    {
        float randomY = Random.Range(0f, 360f);
        transform.Rotate(0f, randomY, 0f);
    }

    void ApplyRandomScale()
    {
        float randomScale = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * randomScale;
    }
}
