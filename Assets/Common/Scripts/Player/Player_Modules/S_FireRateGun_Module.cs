using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(S_EnergyStorage))]
public class S_FireRateGun_Module : MonoBehaviour
{
    [System.Serializable]
    public class FireRateLevel
    {
        public int level; // Niveau requis pour ce niveau de tir
        public float fireRate; // Cadence de tir par seconde
        public float damage; // Dégâts par tir
        public float weekPointMultiplier = 100f;
        public float energyConsumption; // Consommation d'énergie par tir
        public int dropBonus;
        public float knockbackForce;
    }

    [Header("Shooting Settings")]
    public Transform shootPoint; // Point d'origine de la direction de tir
    public Transform spawnBulletPoint;
    public GameObject bulletPrefab;
    public GameObject bulletPalier4Prefab;
    public LayerMask obstacleLayer; // Layer des obstacles
    public float raycastLength = 50f; // Longueur maximale de la portée du tir
    public float raycastSpread = 5f; // Angle de déviation des rayons secondaires
    public bool simulateBulletSpeed = false; // Si vrai, simule une vitesse de balle
    public float bulletSpeed = 20f;

    [Header("Fire Rate Levels")]
    public List<FireRateLevel> fireRateLevels; // Liste des niveaux de tir
    
    private Vector3 _originalPosition; // Position initiale de l'arme
    private Quaternion _originalRotation; // Rotation initiale de l'arme

    private S_InputManager _inputManager;
    private S_EnergyStorage _energyStorage;
    private float _fireCooldown;

    public GameObject HitMarkerPNG;

    public event Action<Enum, int> OnShootStateChange;
    private bool shootStartedUseForEvent;
    private bool ShootStoppedUseForEvent;
    private bool ShootPrepared;
    private bool coroutineStarted;

    private void Start()
    {
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();

        ShootStoppedUseForEvent = true;

    }

    private void Update()
    {
        HandleShooting();
    }

    private void ShootingObserverEvent(Enum Shootstates, FireRateLevel currentLevel)
    {
        OnShootStateChange?.Invoke(Shootstates, currentLevel.level);
    }
    
    private void HandleShooting()
    {
        if (_inputManager.ShootInput && _fireCooldown <= 0f && S_PlayerStateObserver.Instance.LastMeleeState==null)
        {
            FireRateLevel currentLevel = GetCurrentFireRateLevel();
            if (currentLevel == null) return;
            
            
            //Trigger pre-fire delay
            if (!coroutineStarted)
            {
                StartCoroutine(PrepareShoot());

            }
            // return if the delay is not done
            if (!shootStartedUseForEvent)
            {
                return;
            }
            
            ShootingObserverEvent(PlayerStates.ShootState.IsShooting,GetCurrentFireRateLevel());
            Shoot(currentLevel);
            
            //reset stop shot to prepare trigger stop shoot event
            ShootStoppedUseForEvent = false;
            
            SoundManager.Instance.Meth_Shoot_No_Hit(_energyStorage.currentLevelIndex+1);
            UpdateFireCooldown(currentLevel);
            ConsumeEnergy(currentLevel);
        }

        //Trigger once stop shooting event
        if (!_inputManager.ShootInput||S_PlayerStateObserver.Instance.LastMeleeState!=null)
        {
            if (!ShootStoppedUseForEvent)
            {
                ShootingObserverEvent(PlayerStates.ShootState.StopShoot,GetCurrentFireRateLevel());
                ShootStoppedUseForEvent=true;
                coroutineStarted = false;
                shootStartedUseForEvent=false;
            }

        }

        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }
    }
    
    
    // start pre fire delay and trigger start shoot
    private IEnumerator PrepareShoot()
    {
        coroutineStarted = true;
        ShootingObserverEvent(PlayerStates.ShootState.StartShoot,GetCurrentFireRateLevel());
        yield return new WaitForSecondsRealtime(0.05f);
        shootStartedUseForEvent = true;

    }
    
    private void Shoot(FireRateLevel currentLevel)
    {
        Vector3 shootDirection = shootPoint.forward;

        if (simulateBulletSpeed)
        {
            StartCoroutine(SimulateBullet(shootDirection, currentLevel));
        }
        else
        {
            PerformSpreadRaycast(shootPoint.position, shootDirection, raycastLength, currentLevel.damage);
        }

        if (_energyStorage.currentLevelIndex + 1 == 4)
        {
            GameObject BulletPalier4 = Instantiate(bulletPalier4Prefab, spawnBulletPoint.position, spawnBulletPoint.rotation);
            BulletPalier4.GetComponent<S_Projectile_useForDeco>().InitializeProjectile(3, 800);
        }
        else
        {
            GameObject Bullet = Instantiate(bulletPrefab, spawnBulletPoint.position, spawnBulletPoint.rotation);
            Bullet.GetComponent<S_Projectile_useForDeco>().InitializeProjectile(3, bulletSpeed);
        }
    }

    private IEnumerator SimulateBullet(Vector3 shootDirection, FireRateLevel currentLevel)
    {
        float traveledDistance = 0f;

        while (traveledDistance < raycastLength)
        {
            Vector3 currentPosition = shootPoint.position + shootDirection * traveledDistance;

            float step = bulletSpeed * Time.deltaTime;
            traveledDistance += step;

            // Effectuer un raycast dispersé pour chaque étape
            if (PerformSpreadRaycast(currentPosition, shootDirection, step, currentLevel.damage))
            {
                yield break;
            }

            yield return null;
        }
    }

    private bool PerformSpreadRaycast(Vector3 origin, Vector3 direction, float length, float damage)
    {
        // Définir les positions pour le raycast principal et les rayons auxiliaires
        Vector3[] offsets = new Vector3[5];
        offsets[0] = Vector3.zero; // Pas de décalage pour le raycast principal
        offsets[1] = new Vector3(-raycastSpread, raycastSpread, 0); // Haut-gauche
        offsets[2] = new Vector3(raycastSpread, raycastSpread, 0); // Haut-droite
        offsets[3] = new Vector3(-raycastSpread, -raycastSpread, 0); // Bas-gauche
        offsets[4] = new Vector3(raycastSpread, -raycastSpread, 0); // Bas-droite

        HashSet<GameObject> hitTargets = new HashSet<GameObject>();

        bool hitObstacle = false;

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector3 offsetOrigin = origin + shootPoint.TransformDirection(offsets[i]);

            // Dessiner un raycast de debug (bleu par défaut)
            Debug.DrawRay(offsetOrigin, direction * length, Color.blue, 1f);

            // Si le raycast principal touche un obstacle, arrêter le tir
            if (i == 0)
            {
                if (PerformRaycast(offsetOrigin, direction, length, damage, hitTargets, true))
                {
                    Debug.DrawRay(offsetOrigin, direction * length, Color.red, 1f); // Rouge si un obstacle est touché
                    hitObstacle = true;
                }
            }
            else
            {
                // Rayons auxiliaires ne détectent que les cibles
                PerformRaycast(offsetOrigin, direction, length, damage, hitTargets, false);
            }
        }

        return hitObstacle;
    }

    private bool PerformRaycast(Vector3 origin, Vector3 direction, float length, float damage, HashSet<GameObject> hitTargets, bool checkObstacle)
    {
        // Effectuer le raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, length))
        {
            // Si la cible appartient au layer cible
             EnemyBase enemy = hit.collider.gameObject.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                GameObject enemyObject = enemy.gameObject;

                // Éviter les touches répétées sur la même cible
                if (hitTargets.Add(enemyObject))
                {
                    // HitMarkerEnabler
                    StartCoroutine(HitMarker());

                    // Appliquer les degats
                    if(hit.collider.gameObject.CompareTag("WeakPoint"))
                    {
                        enemy.ReduceHealth(damage*GetCurrentFireRateLevel().weekPointMultiplier,GetCurrentFireRateLevel().dropBonus);
                    }
                    else
                    {
                        enemy.ReduceHealth(damage,GetCurrentFireRateLevel().dropBonus,hit.point);
                    }
                    Vector3 hitDirection = (enemy.transform.position - transform.position).normalized;
                    enemy.GetComponent<Rigidbody>().AddForce(hitDirection*GetCurrentFireRateLevel().knockbackForce, ForceMode.Impulse);
                }

                return true; // Une cible a été touchée
            }

            // Si un obstacle est touché (uniquement pour le raycast principal)
            if (checkObstacle && (1 << hit.collider.gameObject.layer & obstacleLayer) != 0)
            {
                return true; // Obstacle touché
            }
        }

        return false; // Rien n'a été touché
    }

    private void UpdateFireCooldown(FireRateLevel currentLevel)
    {
        _fireCooldown = 1f / currentLevel.fireRate;
    }

    private void ConsumeEnergy(FireRateLevel currentLevel)
    {
        _energyStorage.RemoveEnergy(currentLevel.energyConsumption);
    }

    private FireRateLevel GetCurrentFireRateLevel()
    {
        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Ajustement pour correspondre aux niveaux
        return fireRateLevels.Find(level => level.level == currentLevelIndex);
    }

  
    IEnumerator HitMarker()
    {
        HitMarkerPNG.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        HitMarkerPNG.SetActive(false);
    }
}
