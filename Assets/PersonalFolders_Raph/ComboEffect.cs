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

    [Header("Lissage d'affichage")]
    [Tooltip("Vitesse de lissage pour le compteur de kills")]
    public float killLerpSpeed = 5f;

    // valeur interne lissée pour le compteur de kills
    private float displayedKills = 0f;

    void Update()
    {
        if (comboSystem == null) return;

        // Combo actif
        if (comboSystem.comboActive)
        {
            // Montrer les visuels
            redBarObject?.SetActive(true);
            haloObject?.SetActive(true);
            killText?.gameObject.SetActive(true);
            multiplicateurText?.gameObject.SetActive(true);

            // Calcul du ratio combo
            float ratio = Mathf.Clamp01(1f - comboSystem.comboActuelTimer / comboSystem.currentComboSetting.comboTime);
            float mapped = 0.5f + 0.5f * ratio;
            redBarMaterial?.SetFloat(redBarProgressProperty, mapped);

            // MAJ halo (shader)
            haloMaterial?.SetFloat(haloCutoffProperty, ratio);
            haloMaterial?.SetFloat(haloProgressProperty, ratio);

            // —— Lissage du compteur de kills ——
            float targetKills = comboSystem.comboKillCount;
            displayedKills = Mathf.Lerp(displayedKills, targetKills, Time.deltaTime * killLerpSpeed);
            int shownKills = Mathf.FloorToInt(displayedKills + 0.5f);
            if (killText != null)
                killText.text = shownKills.ToString();

            // MAJ multiplicateur (sans lissage)
            if (multiplicateurText != null)
                multiplicateurText.text = $"x{comboSystem.currentComboMultiplier:F2}";
        }
        else
        {
            // Combo inactif → cacher les éléments
            redBarObject?.SetActive(false);
            haloObject?.SetActive(false);
            killText?.gameObject.SetActive(false);
            multiplicateurText?.gameObject.SetActive(false);

            // Réinitialiser le lissage
            displayedKills = 0f;
        }
    }
}