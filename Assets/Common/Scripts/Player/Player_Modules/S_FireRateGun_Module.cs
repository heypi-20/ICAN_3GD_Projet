using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
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
        public float energyConsumption; // Consommation d'énergie par tir
        public int dropBonus;
    }

    [Header("Shooting Settings")]
    public Transform shootPoint; // Point d'origine de la direction de tir
    public Transform spawnBulletPoint;
    public GameObject bulletPrefab;
    public LayerMask obstacleLayer; // Layer des obstacles
    public float raycastLength = 50f; // Longueur maximale de la portée du tir
    public float raycastSpread = 5f; // Angle de déviation des rayons secondaires
    public bool simulateBulletSpeed = false; // Si vrai, simule une vitesse de balle
    public float bulletSpeed = 20f;

    [Header("Fire Rate Levels")]
    public List<FireRateLevel> fireRateLevels; // Liste des niveaux de tir

    [Header("Recoil Settings")]
    public GameObject gunObject; // Référence à l'arme pour appliquer le recul
    public float recoilDistance = 0.1f; // Distance que l'arme recule pendant le tir
    public float recoilDuration = 0.1f; // Durée de l'animation de recul
    public float resetDuration = 0.2f; // Durée pour revenir à la position initiale
    public float upwardRecoilMin = 2f; // Amplitude minimale pour l'élévation du canon
    public float upwardRecoilMax = 5f; // Amplitude maximale pour l'élévation du canon
    public float lateralRecoilMin = -1f; // Amplitude minimale pour le décalage latéral
    public float lateralRecoilMax = 1f; // Amplitude maximale pour le décalage latéral

    private Vector3 _originalPosition; // Position initiale de l'arme
    private Quaternion _originalRotation; // Rotation initiale de l'arme

    private S_InputManager _inputManager;
    private S_EnergyStorage _energyStorage;
    private float _fireCooldown;

    public GameObject HitMarkerPNG;

    public event Action<Enum, int> OnShootStateChange;
    private bool shootStartedUseForEvent;
    private bool ShootStoppedUseForEvent;

    private void Start()
    {
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();

        ShootStoppedUseForEvent = true;
        
        // Initialisation des positions pour le recul
        _originalPosition = gunObject.transform.localPosition;
        _originalRotation = gunObject.transform.localRotation;
    }

    private void Update()
    {
        HandleShooting();
    }

    private void ShootingObserverEvent(Enum Shootstates, FireRateLevel currentLevel)
    {

        if (Shootstates.Equals(PlayerStates.ShootState.StartShoot))
        {
            if (!shootStartedUseForEvent)
            {
                OnShootStateChange?.Invoke(PlayerStates.ShootState.StartShoot, currentLevel.level);
                OnShootStateChange?.Invoke(PlayerStates.ShootState.IsShooting,currentLevel.level);
                shootStartedUseForEvent = true;
            }
            else if (shootStartedUseForEvent)
            {
                OnShootStateChange?.Invoke(PlayerStates.ShootState.IsShooting,currentLevel.level);
            }
        }

        if (Shootstates.Equals(PlayerStates.ShootState.StopShoot))
        {
            shootStartedUseForEvent = false;
            OnShootStateChange?.Invoke(PlayerStates.ShootState.StopShoot, currentLevel.level);
        }
        
        
    }

    private void HandleShooting()
    {
        if (_inputManager.ShootInput && _fireCooldown <= 0f)
        {
            FireRateLevel currentLevel = GetCurrentFireRateLevel();
            if (currentLevel == null) return;
            Shoot(currentLevel);
            
            //Trigger ShootEvent
            ShootingObserverEvent(PlayerStates.ShootState.StartShoot,GetCurrentFireRateLevel());
            ShootStoppedUseForEvent = false;
            
            SoundManager.Instance.Meth_Shoot_No_Hit(_energyStorage.currentLevelIndex+1);
            UpdateFireCooldown(currentLevel);
            ConsumeEnergy(currentLevel);
            ShootVFX();
        }

        //Trigger once stop shooting event
        if (!_inputManager.ShootInput)
        {
            if (!ShootStoppedUseForEvent)
            {
                ShootingObserverEvent(PlayerStates.ShootState.StopShoot,GetCurrentFireRateLevel());
                ShootStoppedUseForEvent=true;
            }

        }

        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }
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

        GameObject Bullet = Instantiate(bulletPrefab, spawnBulletPoint.position, spawnBulletPoint.rotation);
        Bullet.GetComponent<S_Projectile_useForDeco>().InitializeProjectile(3,bulletSpeed);
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
            if (hit.collider.gameObject.GetComponent<EnemyBase>())
            {
                var target = hit.collider.gameObject;

                // Éviter les touches répétées sur la même cible
                if (hitTargets.Add(target))
                {
                    // HitMarkerEnabler
                    StartCoroutine(HitMarker());

                    // Appliquer les degats
                    target.GetComponent<EnemyBase>()?.ReduceHealth(damage,GetCurrentFireRateLevel().dropBonus);
                }

                return true; // Une cible a été touchée
            }
            //else
            //{
            //    CrossAir.color = Color.white;
            //}

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

    private void ShootVFX()
    {
        DOTween.Kill(gunObject.transform);

        // Appliquer un recul visuel à l'arme
        gunObject.transform.DOLocalMove(_originalPosition - gunObject.transform.forward * recoilDistance, recoilDuration)
            .SetEase(Ease.OutQuad);

        // Appliquer une rotation pour simuler le recul
        Vector3 recoilRotation = new Vector3(
            _originalRotation.eulerAngles.x + Random.Range(upwardRecoilMin, upwardRecoilMax),
            _originalRotation.eulerAngles.y + Random.Range(lateralRecoilMin, lateralRecoilMax),
            _originalRotation.eulerAngles.z);

        gunObject.transform.DOLocalRotate(recoilRotation, recoilDuration)
            .SetEase(Ease.OutQuad);

        // Retour à la position initiale après le recul
        gunObject.transform.DOLocalMove(_originalPosition, resetDuration)
            .SetDelay(recoilDuration)
            .SetEase(Ease.InQuad);

        // Retour à l'orientation initiale après le recul
        gunObject.transform.DOLocalRotate(_originalRotation.eulerAngles, resetDuration)
            .SetDelay(recoilDuration)
            .SetEase(Ease.InQuad);
    }

    IEnumerator HitMarker()
    {
        HitMarkerPNG.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        HitMarkerPNG.SetActive(false);
    }
}
