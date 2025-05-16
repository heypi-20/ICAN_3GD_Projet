using System;
using System.Collections;
using UnityEngine;

public class S_PlayerProjectileSpawner : MonoBehaviour
{
    /* ──────────── PROJECTILES ──────────── */
    [Header("Projectile Prefabs / Activation Targets")]
    public GameObject ProjectilePrefab_Lv1;
    public GameObject ProjectilePrefab_Lv2;
    public GameObject ProjectilePrefab_Lv3;
    public GameObject ProjectilePrefab_Lv4;

    [Header("Activation Mode (beam / laser)")]
    public bool useActivationLv1;
    public bool useActivationLv2;
    public bool useActivationLv3;
    public bool useActivationLv4;

    /* ──────────── MUZZLE FLASH ──────────── */
    [Header("Muzzle-Flash (VFX)")]
    [Tooltip("Empty parent avec tes 2 ParticleSystem enfants")]
    public GameObject MuzzleFlashPrefab;

    /* ──────────── TRAJECTOIRE ──────────── */
    [Space(10)]
    public float Speed = 200f;
    public float ProjectileLifeTime = 3f;
    public float FollowTime = 0.1f;
    public AnimationCurve SpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    /* ──────────── PRIVATE ──────────── */
    private GameObject activationInstanceLv1;
    private GameObject activationInstanceLv2;
    private GameObject activationInstanceLv3;
    private GameObject activationInstanceLv4;

    /* ──────────── UNITY EVENTS ──────────── */
    private void Start()
    {
        S_PlayerStateObserver.Instance.OnShootStateEvent += OnShoot;
    }

    private void OnDestroy()
    {
        if (S_PlayerStateObserver.Instance != null)
            S_PlayerStateObserver.Instance.OnShootStateEvent -= OnShoot;
    }

    /* ──────────── SHOOT CALLBACK ──────────── */
    private void OnShoot(Enum state, int level)
    {
        bool isShooting = state.Equals(PlayerStates.ShootState.IsShooting);

        switch (level)
        {
            case 1:
                HandleLevel(ProjectilePrefab_Lv1, useActivationLv1,
                            ref activationInstanceLv1, isShooting);
                break;
            case 2:
                HandleLevel(ProjectilePrefab_Lv2, useActivationLv2,
                            ref activationInstanceLv2, isShooting);
                break;
            case 3:
                HandleLevel(ProjectilePrefab_Lv3, useActivationLv3,
                            ref activationInstanceLv3, isShooting);
                break;
            case 4:
                HandleLevel(ProjectilePrefab_Lv4, useActivationLv4,
                            ref activationInstanceLv4, isShooting);
                break;
            default:
                Debug.LogWarning($"Invalid projectile level: {level}");
                break;
        }
    }

    /* ──────────── LEVEL HANDLER ──────────── */
    private void HandleLevel(GameObject prefab, bool useActivation,
                             ref GameObject instance, bool isShooting)
    {
        if (prefab == null) return;

        if (useActivation)
        {
            /* ------- Mode activation (beam / laser) ------- */
            if (isShooting)
            {
                if (instance == null)
                {
                    instance = Instantiate(prefab, transform.position,
                                           transform.rotation, transform);
                }
                instance.SetActive(true);
                SpawnMuzzleFlash();                                  // ← NEW
            }
            else if (instance != null)
            {
                instance.SetActive(false);
            }
        }
        else
        {
            /* ------- Un projectile par tir ------- */
            if (isShooting)
            {
                var proj = Instantiate(prefab, transform.position,
                                       transform.rotation, transform);
                StartCoroutine(MoveAndDestroy(proj));
                SpawnMuzzleFlash();                                  // ← NEW
            }
        }
    }

    /* ──────────── MUZZLE FLASH SPAWN ──────────── */
    private void SpawnMuzzleFlash()
    {
        if (MuzzleFlashPrefab == null) return;

        // L’instantiation suit l’arme ; pas besoin de Destroy(),
        // les ParticleSystem s’auto-suppriment.
        Instantiate(MuzzleFlashPrefab,
                    transform.position,
                    transform.rotation,
                    transform);
    }

    /* ──────────── PROJECTILE TRAJECTOIRE ──────────── */
    private IEnumerator MoveAndDestroy(GameObject projectile)
    {
        float elapsed = 0f;
        Transform t = projectile.transform;

        while (elapsed < ProjectileLifeTime)
        {
            float pct = elapsed / ProjectileLifeTime;
            float speed = Speed * SpeedCurve.Evaluate(pct);

            if (elapsed < FollowTime)
            {
                t.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
            }
            else
            {
                if (t.parent != null) t.SetParent(null);
                t.Translate(t.forward * speed * Time.deltaTime, Space.World);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(projectile);
    }
}