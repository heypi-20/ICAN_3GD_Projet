using UnityEngine;
using System.Collections;
using TMPro;

public class DemoGaugeAnimator : MonoBehaviour
{
    [Header("Réf. Matériel jauge")]
    public Material gaugeMaterial;
    public string gaugeProperty = "_Progress";  // 0.49 = plein → 1 = vide

    [Header("Textes 3D")]
    public TextMeshPro killText;
    public TextMeshPro multiplicateurText;

    [Header("DOTween")]
    public S_DotweenPlayer dotweenPlayer;

    [Header("Durée démo")]
    [Tooltip("Durée totale de la démo (secondes)")]
    public float demoDuration = 60f;

    [Header("Cycle aléatoire jauge")]
    [Tooltip("Durée min du vidage (secondes)")]
    public float minEmptyDuration = 1f;
    [Tooltip("Durée max du vidage (secondes)")]
    public float maxEmptyDuration = 5f;
    [Tooltip("Niveau minimal de la jauge (plein)")]
    [Range(0f, 1f)] public float minEmptyLevel = 0.49f;
    [Tooltip("Niveau maximal de la jauge (vide)")]
    [Range(0f, 1f)] public float maxEmptyLevel = 0.8f;

    [Header("Simulation kills")]
    [Tooltip("Kills simulés min par cycle")]
    public int minKillsPerCycle = 1;
    [Tooltip("Kills simulés max par cycle")]
    public int maxKillsPerCycle = 3;
    [Tooltip("Pause entre chaque kill simulé (s)")]
    public float killInterval = 0.1f;

    [Header("Simulation combo")]
    [Tooltip("Incrément multip à chaque N cycles")]
    public float comboIncreaseAmount = 0.2f;
    [Tooltip("Nombre de cycles avant d’incrémenter le combo")]
    public int cyclesPerComboIncrease = 2;
    [Tooltip("Multiplicateur de départ")]
    public float startMultiplier = 1f;

    // état interne
    private int fakeKillCount;
    private float fakeMultiplier;
    private int cycleCount;
    private float elapsed;

    void Start()
    {
        if (gaugeMaterial == null || killText == null || multiplicateurText == null)
        {
            Debug.LogError("Assigne tous les champs dans l'Inspector !");
            enabled = false;
            return;
        }

        // Initialisation
        fakeKillCount = 0;
        fakeMultiplier = startMultiplier;
        cycleCount = 0;
        elapsed = 0f;

        killText.text = "0";
        multiplicateurText.text = $"x{fakeMultiplier:F2}";
        gaugeMaterial.SetFloat(gaugeProperty, minEmptyLevel);

        StartCoroutine(PlayDemo());
    }

    private IEnumerator PlayDemo()
    {
        while (elapsed < demoDuration)
        {
            // 1) Vidage sur durée aléatoire
            float targetEmpty = Random.Range(minEmptyLevel, maxEmptyLevel);
            float durEmpty = Random.Range(minEmptyDuration, maxEmptyDuration);
            float t = 0f;

            while (t < durEmpty && elapsed < demoDuration)
            {
                float dt = Time.deltaTime;
                t += dt;
                elapsed += dt;
                // Reveal = 0.49 → targetEmpty
                float r = Mathf.Lerp(minEmptyLevel, targetEmpty, t / durEmpty);
                gaugeMaterial.SetFloat(gaugeProperty, r);
                yield return null;
            }

            // On vient de terminer un cycle de vidage
            cycleCount++;

            // 2) Lance la simulation de kills en parallèle (n’empêche plus la jauge)
            int killsThisCycle = Random.Range(minKillsPerCycle, maxKillsPerCycle + 1);
            StartCoroutine(KillSimulation(killsThisCycle));

            // 3) Refill instantané
            gaugeMaterial.SetFloat(gaugeProperty, minEmptyLevel);

            // 4) Combo : tous les X cycles
            if (cycleCount % cyclesPerComboIncrease == 0)
            {
                fakeMultiplier += comboIncreaseAmount;
                multiplicateurText.text = $"x{fakeMultiplier:F2}";
            }
        }
    }

    private IEnumerator KillSimulation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            fakeKillCount++;
            killText.text = fakeKillCount.ToString();
            dotweenPlayer?.Play();
            yield return new WaitForSeconds(killInterval);
        }
    }
}
