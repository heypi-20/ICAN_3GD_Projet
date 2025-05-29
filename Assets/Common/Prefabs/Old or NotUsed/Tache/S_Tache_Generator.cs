using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Tache_Generator : MonoBehaviour
{
    [Header("Matériaux")]
    [Tooltip("Liste des matériaux possibles à appliquer.")]
    public Material[] materials;

    [Header("Échelle aléatoire")]
    [Tooltip("Échelle minimale (uniforme)")]
    public float minScale = 1f;
    [Tooltip("Échelle maximale (uniforme)")]
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
            // Si plusieurs MeshRenderers (enfants), appliquer à tous
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
