using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ComboEffect : MonoBehaviour
{
    [Header("Références système combo")]
    public S_ComboSystem comboSystem;

    [Header("Barre visuelle")]
    public GameObject redBarObject; // GameObject contenant la barre rouge
    public Material redBarMaterial; // Shader "ComboJauge"
    public string redBarProgressProperty = "_Progress";

    [Header("Halo")]
    public GameObject haloObject; // GameObject contenant le halo (barre grise)
    public Material haloMaterial; // Shader "RotatingHaloMasked"
    public string haloCutoffProperty = "_CutoffX";
    public string haloProgressProperty = "_Progress"; // masque aussi avec _Progress

    [Header("Textes 3D")]
    public TextMeshPro killText;
    public TextMeshPro multiplicateurText;

    void Update()
    {
        if (comboSystem == null) return;

        // Combo actif
        if (comboSystem.comboActive)
        {
            // Montrer les visuels
            if (redBarObject != null) redBarObject.SetActive(true);
            if (haloObject != null) haloObject.SetActive(true);
            if (killText != null) killText.gameObject.SetActive(true);
            if (multiplicateurText != null) multiplicateurText.gameObject.SetActive(true);

            // Calcul du ratio combo
            float ratio = Mathf.Clamp01(1f - comboSystem.comboActuelTimer / comboSystem.currentComboSetting.comboTime);
            float mapped = 0.5f + 0.5f * ratio;
            redBarMaterial?.SetFloat(redBarProgressProperty, mapped);
            Debug.Log("rqtio:" + mapped);
            // MAJ jauge rouge (shader)

            // MAJ halo (shader)
            if (haloMaterial != null)
            {
                haloMaterial.SetFloat(haloCutoffProperty, ratio);
                haloMaterial.SetFloat(haloProgressProperty, ratio);
            }

            // MAJ textes
            if (killText != null)
                killText.text = $" {comboSystem.comboKillCount}";

            if (multiplicateurText != null)
                multiplicateurText.text = $"x{comboSystem.currentComboMultiplier:F2}";
        }
        else
        {
            // Combo inactif → cacher les éléments
            if (redBarObject != null) redBarObject.SetActive(false);
            if (haloObject != null) haloObject.SetActive(false);
            if (killText != null) killText.gameObject.SetActive(false);
            if (multiplicateurText != null) multiplicateurText.gameObject.SetActive(false);
        }
    }
}
