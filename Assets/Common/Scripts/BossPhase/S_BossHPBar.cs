using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [Header("Boss Settings")]
    [Tooltip("Drag the boss GameObject here (must have LaserShooterBoss component)")]
    public GameObject boss;

    [Header("Health Bar UI")]
    [Tooltip("Drag the UI Image component here (must have a Material with _Progress)")]
    public Image hpBarImage;

    // References
    private S_LaserShooterBoss laserShooterBoss;
    private Material           hpMaterial;

    void Start()
    {
        // 1. Get the boss health script
        laserShooterBoss = boss.GetComponent<S_LaserShooterBoss>();
        if (laserShooterBoss == null)
            Debug.LogError("Boss object is missing LaserShooterBoss component!");

        // 2. Get the Image's material instance (use .material to avoid editing shared asset)
        if (hpBarImage == null)
        {
            Debug.LogError("hpBarImage reference is null!");
        }
        else
        {
            hpMaterial = hpBarImage.material;
        }
    }

    void Update()
    {
        if (laserShooterBoss == null || hpMaterial == null)
            return;

        // 3. Calculate fill progress: 0 = full health, 1 = zero health
        float maxHP    = laserShooterBoss.health;     // Maximum health
        float curHP    = laserShooterBoss.currentHealth; // Current health
        float fillNorm = curHP / maxHP;                  // 1 = full → 0 = empty
        float progress = 1f - fillNorm;                  // 0 = full → 1 = empty
        progress = Mathf.Clamp01(progress);

        // 4. Assign to shader's _Progress parameter
        hpMaterial.SetFloat("_Progress", progress);
    }
}