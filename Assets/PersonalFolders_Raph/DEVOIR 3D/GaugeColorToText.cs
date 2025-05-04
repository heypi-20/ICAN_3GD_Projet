using UnityEngine;
using TMPro;

[RequireComponent(typeof(DemoGaugeAnimator))]
public class GaugeColorToText : MonoBehaviour
{
    [Header("Référence au DemoGaugeAnimator")]
    public DemoGaugeAnimator demo;

    [Header("Textes à colorer")]
    public TextMeshPro killText;
    public TextMeshPro multiplicateurText;

    [Header("Propriétés shader de la jauge")]
    public string colorStartProp = "_ColorStart";
    public string colorEndProp = "_ColorEnd";
    public string progressProp = "_Progress";

    // IDs shader
    private int idStart, idEnd, idProg, idUnderlay;

    void Awake()
    {
        // On ne touche à rien de TMP avant Awake
        idStart = Shader.PropertyToID(colorStartProp);
        idEnd = Shader.PropertyToID(colorEndProp);
        idProg = Shader.PropertyToID(progressProp);
        idUnderlay = Shader.PropertyToID("_UnderlayColor");

        // Clonez la material du texte pour ne pas écraser le sharedMaterial
        if (killText != null)
            killText.fontMaterial = new Material(killText.fontMaterial);
        if (multiplicateurText != null)
            multiplicateurText.fontMaterial = new Material(multiplicateurText.fontMaterial);
    }

    void Update()
    {
        if (demo == null || demo.gaugeMaterial == null) return;

        // Lecture de la progression et des couleurs dans la material de la jauge
        var mat = demo.gaugeMaterial;
        float t = mat.GetFloat(idProg);
        Color c0 = mat.GetColor(idStart);
        Color c1 = mat.GetColor(idEnd);
        Color col = Color.Lerp(c0, c1, t);

        // Applique en Underlay des textes
        if (killText != null)
            killText.fontMaterial.SetColor(idUnderlay, col);
        if (multiplicateurText != null)
            multiplicateurText.fontMaterial.SetColor(idUnderlay, col);
    }
}
