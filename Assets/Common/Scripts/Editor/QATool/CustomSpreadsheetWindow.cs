using UnityEngine;
using System.Collections;
using TMPro;

public class DemoGaugeAnimator : MonoBehaviour
{
    [Header("Réf. Matériel jauge")]
    public Material gaugeMaterial;
    public string gaugeProperty = "_Progress";  // 0.49 plein → 1 vide

    [Header("Textes 3D")]
    public TextMeshPro killText;
    public TextMeshPro multiplicateurText;

    [Header("DOTween")]
    public S_DotweenPlayer dotweenPlayer;

    [Header("Durée démo")]
    [Tooltip("Durée totale de la démo en secondes")]
    public float demoDuration = 60f;

    [Header("Cycle aléatoire jauge")]
    [Tooltip("Durée min du vidage (secondes)")]
    public float minEmptyDuration = 1f;
    [Tooltip("Durée max du vidage (secondes)")]
    public float maxEmptyDuration = 5f;
    [Range(0f, 1f)]
    [Tooltip("Niveau max de vide (borne supérieure du Lerp)")]
    public float maxEmptyLevel = 0.8f;

    [Header("Simulation kills")]
    [Tooltip("Kills simulés min par cycle")]
    public int minKillsPerCycle = 1;
    [Tooltip("Kills simulés max par cycle")]
    public int maxKillsPerCycle = 3;
    [Tooltip("Pause entre chaque kill simulé (secondes)")]
    public float killInterval = 0.1f;

    [Header("Simulation combo")]
    [Tooltip("Incrément multip lorsque le seuil est atteint")]
    public float comboIncreaseAmount = 0.2f;
    [Tooltip("Nombre de cycles de vidage avant d’incrémenter le combo")]
    public int cyclesPerComboIncrease = 2;
    [Tooltip("Valeur de départ du combo")]
    public float startMultiplier = 1f;

    // État interne
    private int fakeKillCount;
    private float fakeMultiplier;
    private int cycleCount;

    void Start()
    {
        // Validation rapide
        if (gaugeMaterial == null || killText == null || multiplicateurText == null)
        {
            Debug.LogError("Assigne Gauge Material + Textes dans l'Inspector !");
            enabled = false;
            return;
        }

        fakeKillCount = 0;
        fakeMultiplier = startMultiplier;
        cycleCount = 0;

        // Initial UI
        killText.text = "0";
        multiplicateurText.text = $"x{fakeMultiplier:F2}";

        StartCoroutine(PlayDemo());
    }

    private IEnumerator PlayDemo()
    {
        float elapsed = 0f;

        while (elapsed < demoDuration)
        {
            // — VIDAGE de la jauge (0.49 → random emptyLevel) —
            float emptyLevel = Random.Range(0f, maxEmptyLevel);
            float durEmpty = Random.Range(minEmptyDuration, maxEmptyDuration);
            float t = 0f;

            while (t < durEmpty && elapsed < demoDuration)
            {
                float dt = Time.deltaTime;
                t += dt;
                elapsed += dt;

                float reveal = Mathf.Lerp(0.49f, emptyLevel, t / durEmpty);
                gaugeMaterial.SetFloat(gaugeProperty, reveal);

                yield return null;
            }

            // Incrément du compteur de cycles
            cycleCount++;

            // — SIMULATION des kills pour ce cycle —
            int killsThisCycle = Random.Range(minKillsPerCycle, maxKillsPerCycle + 1);
            for (int k = 0; k < killsThisCycle; k++)
            {
                fakeKillCount++;
                killText.text = fakeKillCount.ToString();
                dotweenPlayer?.Play();
                yield return new WaitForSeconds(killInterval);
            }

            // — Refill instantané à plein (0.49) —
            gaugeMaterial.SetFloat(gaugeProperty, 0.49f);

            // — Incrément du combo seulement tous les N cycles —
            if (cycleCount % cyclesPerComboIncrease == 0)
            {
                fakeMultiplier += comboIncreaseAmount;
                multiplicateurText.text = $"x{fakeMultiplier:F2}";
            }
        }
    }
}
