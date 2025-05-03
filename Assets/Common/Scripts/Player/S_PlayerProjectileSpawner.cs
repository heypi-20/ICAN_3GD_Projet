using System.Collections;
using UnityEngine;
using System;

public class S_PlayerProjectileSpawner : MonoBehaviour
{
    // Prefabs for projectiles at different levels
    public GameObject ProjectilePrefab_Lv1;
    public GameObject ProjectilePrefab_Lv2;
    public GameObject ProjectilePrefab_Lv3;
    public GameObject ProjectilePrefab_Lv4;

    [Space(20)]
    // Base movement speed of the projectile
    public float Speed = 200f;
    // Total lifetime of the projectile before destruction
    public float ProjectileLifeTime = 3f;
    [Tooltip("Duration (in seconds) that the projectile follows the spawner")]
    public float FollowTime = 0.1f;
    [Tooltip("Curve to control speed multiplier over lifetime (0=start, 1=end)")]
    public AnimationCurve SpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        // Subscribe to the player's shooting event
        S_PlayerStateObserver.Instance.OnShootStateEvent += OnShoot;
    }

    private void OnShoot(Enum state, int level)
    {
        // When the player enters the shooting state, spawn a projectile of the given level
        if (state.Equals(PlayerStates.ShootState.IsShooting))
            SpawnProjectile(level);
    }

    private void SpawnProjectile(int level)
    {
        // Select the correct prefab based on the projectile level
        GameObject prefabToSpawn = null;
        switch (level)
        {
            case 1:
                prefabToSpawn = ProjectilePrefab_Lv1;
                break;
            case 2:
                prefabToSpawn = ProjectilePrefab_Lv2;
                break;
            case 3:
                prefabToSpawn = ProjectilePrefab_Lv3;
                break;
            case 4:
                prefabToSpawn = ProjectilePrefab_Lv4;
                break;
            default:
                Debug.LogWarning($"Invalid projectile level: {level}");
                return;
        }

        // Instantiate the projectile as a child of this spawner to enable initial follow
        GameObject projectile = Instantiate(
            prefabToSpawn,
            transform.position,
            transform.rotation,
            transform // Set as child for initial follow phase
        );

        // Start coroutine to handle movement with speed curve and timed destruction
        StartCoroutine(MoveAndDestroy(projectile));
    }

    private IEnumerator MoveAndDestroy(GameObject projectile)
    {
        float elapsed = 0f;
        Transform t = projectile.transform;

        // Continue until the projectile's lifetime expires
        while (elapsed < ProjectileLifeTime)
        {
            // Evaluate speed multiplier from the curve (normalized time from 0 to 1)
            float normalizedTime = elapsed / ProjectileLifeTime;
            float speedMultiplier = SpeedCurve.Evaluate(normalizedTime);
            float currentSpeed = Speed * speedMultiplier;

            if (elapsed < FollowTime)
            {
                // Phase 1: move with the spawner in local space
                t.Translate(Vector3.forward * (currentSpeed * Time.deltaTime), Space.Self);
            }
            else
            {
                // Phase 2: detach and move independently in world space
                if (t.parent != null&&t!=null)
                    t.SetParent(null);  // Detach from spawner

                // Move forward along its current forward direction in world space
                t.Translate(t.forward * (currentSpeed * Time.deltaTime), Space.World);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy the projectile after its lifetime has passed
        Destroy(projectile);
    }
}
