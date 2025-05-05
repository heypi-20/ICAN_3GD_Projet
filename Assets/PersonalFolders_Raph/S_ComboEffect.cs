using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

[ExecuteAlways]
public class S_ComboEffect : MonoBehaviour
{
    [Header("Références système combo")]
    public S_ComboSystem comboSystem;

    [Header("Barre visuelle")]
    public GameObject redBarObject; // GameObject contenant la barre rouge
    public string redBarProgressProperty = "_Progress";

    [Header("Textes 3D")]
    public TextMeshPro killText;
    public TextMeshPro multiplicateurText;

    [Header("Lissage d'affichage")]
    [Tooltip("Vitesse de lissage pour le compteur de kills")]
    public float killLerpSpeed = 5f;

    [Header("Dotween Animation")]
    [Tooltip("Le DOTweenPlayer qui animera le KillText")]
    public S_DotweenPlayer dotweenPlayer;
    [Tooltip("Temps minimal (s) entre deux déclenchements")]
    public float tweenCooldown = 0.1f;

    private int lastKillCount = 0;
    private float lastTweenTime = -Mathf.Infinity;
    private float displayedKills = 0f;

    // --- Nouvelles variables pour l'instance de matériau ---
    private Renderer redBarRenderer;
    private MaterialPropertyBlock propertyBlock;

    void OnEnable()
    {
        if (redBarObject != null)
        {
            redBarRenderer = redBarObject.GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    void Update()
    {
        // En mode éditeur sans Play, on garde l'affichage
        if (!Application.isPlaying)
        {
            redBarObject?.SetActive(true);
            killText?.gameObject.SetActive(true);
            multiplicateurText?.gameObject.SetActive(true);
            return;
        }

        if (comboSystem == null)
            return;

        if (comboSystem.comboActive)
        {
            redBarObject?.SetActive(true);
            killText?.gameObject.SetActive(true);
            multiplicateurText?.gameObject.SetActive(true);

            // Calcul du ratio combo
            float ratio = Mathf.Clamp01(1f - comboSystem.comboActuelTimer / comboSystem.currentComboSetting.comboTime);
            float mapped = 0.5f + 0.5f * ratio;

            // Appliquer la propriété sur l'instance du matériau sans modifier l'asset
            if (redBarRenderer != null)
            {
                redBarRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(redBarProgressProperty, mapped);
                redBarRenderer.SetPropertyBlock(propertyBlock);
            }

            // —— Lissage du compteur de kills ——
            float targetKills = comboSystem.comboKillCount;
            displayedKills = Mathf.Lerp(displayedKills, targetKills, Time.deltaTime * killLerpSpeed);
            int shownKills = Mathf.FloorToInt(displayedKills + 0.5f)+1;
            if (killText != null)
                killText.text = shownKills.ToString();

            if (comboSystem.comboKillCount > lastKillCount
                && Time.time - lastTweenTime >= tweenCooldown
                && dotweenPlayer != null)
            {
                dotweenPlayer.Play();
                lastTweenTime = Time.time;
            }
            lastKillCount = comboSystem.comboKillCount;

            // MAJ multiplicateur (sans lissage)
            if (multiplicateurText != null)
                multiplicateurText.text = $"x{comboSystem.currentComboMultiplier:F2}";
        }
        else
        {
            redBarObject?.SetActive(false);
            killText?.gameObject.SetActive(false);
            multiplicateurText?.gameObject.SetActive(false);
            displayedKills = 0f;
        }
    }
}