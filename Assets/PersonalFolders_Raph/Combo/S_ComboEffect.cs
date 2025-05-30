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
    public GameObject redBarObject;
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

    [Header("Multiplier Pulse Animation")]
    [Tooltip("Speed of the pulsing effect (cycles per second)")]
    public float multiplierPulseSpeed = 1.5f;
    [Tooltip("How far above/below 1× scale the pulse goes (e.g. 0.05 = ±5%)")]
    public float multiplierPulseAmount = 0.05f;

    private int lastKillCount = 0;
    private float lastTweenTime = -Mathf.Infinity;
    private float displayedKills = 0f;

    // Material instance
    private Renderer redBarRenderer;
    private MaterialPropertyBlock propertyBlock;

    // Cache the original scale of the multiplier text
    private Vector3 multiplicateurBaseScale;

    void OnEnable()
    {
        if (redBarObject != null)
        {
            redBarRenderer = redBarObject.GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        if (multiplicateurText != null)
            multiplicateurBaseScale = multiplicateurText.transform.localScale;
    }

    void Update()
    {
        // In editor, always show
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
            // -- show UI
            redBarObject?.SetActive(true);
            killText?.gameObject.SetActive(true);
            multiplicateurText?.gameObject.SetActive(true);

            // -- update red bar
            float ratio = Mathf.Clamp01(1f - comboSystem.comboActuelTimer / comboSystem.currentComboSetting.comboTime);
            float mapped = 0.5f + 0.5f * ratio;
            if (redBarRenderer != null)
            {
                redBarRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(redBarProgressProperty, mapped);
                redBarRenderer.SetPropertyBlock(propertyBlock);
            }

            // -- smooth kill counter
            float targetKills = comboSystem.comboKillCount;
            displayedKills = Mathf.Lerp(displayedKills, targetKills, Time.deltaTime * killLerpSpeed);
            int shownKills = Mathf.FloorToInt(displayedKills + 0.5f) + 1;
            if (killText != null)
                killText.text = shownKills.ToString();

            // -- kill tween
            if (comboSystem.comboKillCount > lastKillCount
                && Time.time - lastTweenTime >= tweenCooldown
                && dotweenPlayer != null)
            {
                dotweenPlayer.Play();
                lastTweenTime = Time.time;
            }
            lastKillCount = comboSystem.comboKillCount;

            // -- multiplier text (as percent + pts + EN)
            if (multiplicateurText != null)
                multiplicateurText.text = $"x{comboSystem.currentComboMultiplier * 100f:F0}%";

            // -- now apply a continuous subtle pulse to its scale
            if (multiplicateurText != null)
            {
                float pulse = 1f + multiplierPulseAmount * Mathf.Sin(Time.time * multiplierPulseSpeed * 2f * Mathf.PI);
                multiplicateurText.transform.localScale = multiplicateurBaseScale * pulse;
            }
        }
        else
        {
            // hide UI and reset
            redBarObject?.SetActive(false);
            killText?.gameObject.SetActive(false);
            multiplicateurText?.gameObject.SetActive(false);
            displayedKills = 0f;

            // restore original scale so it’s perfect next combo
            if (multiplicateurText != null)
                multiplicateurText.transform.localScale = multiplicateurBaseScale;
        }
    }
}
