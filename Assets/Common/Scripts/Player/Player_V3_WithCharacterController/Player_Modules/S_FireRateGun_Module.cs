using System.Collections;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_EnergyStorage))]
public class S_FireRateGun_Module : MonoBehaviour
{
    [Header("Shoot Settings")]
    public Transform shootPoint; // Point d'origine de la direction de tir

    public Transform spawnBulletPoint;
    public GameObject bulletPrefab;
    public LayerMask targetLayer; // Layer des cibles destructibles
    public LayerMask obstacleLayer; // Layer des obstacles
    public float raycastLength = 50f; // Longueur maximale de la portée du tir
    public bool simulateBulletSpeed = false; // Si vrai, simule une vitesse de balle
    public float bulletSpeed = 20f;
    
    [Header("Fire Rate Settings")]
    public float minFireRate = 0.5f; // Cadence de tir minimale
    public float maxFireRate = 2f; // Cadence de tir maximale
    public float fireRatePercentage = 0.01f; // Multiplicateur en pourcentage appliqué à currentEnergy
    public float fireRateMultiplier = 1f; // Multiplicateur final appliqué à la cadence calculée

    [Header("Energy Consumption Settings")]
    public float minEnergyConsumption = 5f; // Consommation d'énergie minimale par tir
    public float maxEnergyConsumption = 15f; // Consommation d'énergie maximale par tir
    public float energyConsumptionPercentage = 0.01f; // Multiplicateur en pourcentage appliqué à currentEnergy
    public float energyConsumptionMultiplier = 1f; // Multiplicateur final appliqué à la consommation calculée

    private S_InputManager _inputManager;
    private S_EnergyStorage _energyStorage;
    private float _fireCooldown;

    public float estimatedMaxFireRateThreshold { get; private set; } // Seuil énergétique pour atteindre la cadence maximale
    public float estimatedMinEnergyConsumptionThreshold { get; private set; } // Seuil énergétique pour atteindre la consommation minimale
    private void Start()
    {
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_inputManager.ShootInput && _fireCooldown <= 0f)
        {
            Shoot();
            UpdateFireCooldown();
            ConsumeEnergy();
        }

        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }
    }

    private void Shoot()
    {
        Vector3 shootDirection = shootPoint.forward;
        if (simulateBulletSpeed)
        {
            StartCoroutine(SimulateBullet(shootDirection));
        }
        else
        {
            PerformRaycast(shootPoint.position, shootDirection, raycastLength);
            Debug.DrawRay(shootPoint.position, shootDirection * raycastLength, Color.red, 1f);
        }
        GameObject projectile = Instantiate(bulletPrefab, spawnBulletPoint.position, shootPoint.rotation);
        S_Projectile projectileScript = projectile.GetComponent<S_Projectile>();

        if (projectileScript != null)
        {
            projectileScript.InitializeProjectile(5f,bulletSpeed);
        }
    }
    private IEnumerator SimulateBullet(Vector3 shootDirection)
    {
        float traveledDistance = 0f;

        
        while (traveledDistance < raycastLength)
        {
            Vector3 currentPosition = shootPoint.position + shootDirection * traveledDistance;

            
            float step = bulletSpeed * Time.deltaTime;
            traveledDistance += step;
            Debug.DrawRay(currentPosition, shootDirection * (bulletSpeed * Time.deltaTime), Color.blue, 1f);

            
            if (PerformRaycast(currentPosition, shootDirection, step))
            {
                yield break; 
            }

            yield return null; 
        }
    }
    private bool PerformRaycast(Vector3 origin, Vector3 direction, float length)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, length))
        {
            if ((1 << hit.collider.gameObject.layer & targetLayer) != 0)
            {
                hit.collider.gameObject.GetComponent<S_DroppingModule>().DropItems(5f);
                hit.collider.gameObject.GetComponent<S_DestructionModule>().DestroyObject();
                return true; 
            }

            if ((1 << hit.collider.gameObject.layer & obstacleLayer) != 0)
            {
                
                return true; 
            }
        }

        return false; 
    }

    private void UpdateFireCooldown()
    {
        float calculatedFireRate = (Mathf.Max(_energyStorage.currentEnergy, 0f) * fireRatePercentage) * fireRateMultiplier;
        calculatedFireRate = Mathf.Clamp(calculatedFireRate, minFireRate, maxFireRate);
        _fireCooldown = 1f / calculatedFireRate;
    }

    private void ConsumeEnergy()
    {

        float calculatedConsumption = (Mathf.Max(_energyStorage.currentEnergy, 0f) * energyConsumptionPercentage) * energyConsumptionMultiplier;
        calculatedConsumption = Mathf.Clamp(calculatedConsumption, minEnergyConsumption, maxEnergyConsumption);
        _energyStorage.currentEnergy -= calculatedConsumption;
    }
    private void OnValidate()
    {
        EstimateEnergyThresholds();
    }

    private void EstimateEnergyThresholds()
    {
        if (fireRatePercentage > 0f && fireRateMultiplier > 0f)
        {
            estimatedMaxFireRateThreshold = maxFireRate / (fireRatePercentage * fireRateMultiplier);
        }
        else
        {
            estimatedMaxFireRateThreshold = 0f;
        }

        if (energyConsumptionPercentage > 0f && energyConsumptionMultiplier > 0f)
        {
            estimatedMinEnergyConsumptionThreshold = minEnergyConsumption / (energyConsumptionPercentage * energyConsumptionMultiplier);
        }
        else
        {
            estimatedMinEnergyConsumptionThreshold = 0f;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_FireRateGun_Module))]
public class S_FireRateGunModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        S_FireRateGun_Module module = (S_FireRateGun_Module)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Estimated Energy Thresholds", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Max Fire Rate Threshold:", module.estimatedMaxFireRateThreshold.ToString("F2"));
        EditorGUILayout.LabelField("Min Energy Consumption Threshold:", module.estimatedMinEnergyConsumptionThreshold.ToString("F2"));
    }
}
#endif
