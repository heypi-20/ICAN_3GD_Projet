using System.Collections;
using UnityEngine;
using System;

public class S_PlayerProjectileSpawner : MonoBehaviour
{
    public GameObject ProjectilePrefab_Lv1;
    public GameObject ProjectilePrefab_Lv2;
    public GameObject ProjectilePrefab_Lv3;
    public GameObject ProjectilePrefab_Lv4;

    [Space(20)]
    public float Speed = 200f;
    public float ProjectileLifeTime = 3f;

    private void Start()
    {
        // Subscribe to the shooting event
        S_PlayerStateObserver.Instance.OnShootStateEvent += OnShoot;
    }

    private void OnShoot(Enum state, int level)
    {
        // When the player is shooting, spawn a projectile of the given level
        if (state.Equals(PlayerStates.ShootState.IsShooting))
        {
            SpawnProjectile(level);
        }
    }

    private void SpawnProjectile(int level)
    {
        // Select the appropriate prefab based on level
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

        // Instantiate the projectile at this object’s position and rotation
        GameObject projectile = Instantiate(prefabToSpawn, transform.position, transform.rotation);

        // Start movement + self-destruction coroutine
        StartCoroutine(MoveAndDestroy(projectile));
    }

    private IEnumerator MoveAndDestroy(GameObject projectile)
    {
        float elapsed = 0f;

        // Move forward until lifetime expires
        while (elapsed < ProjectileLifeTime)
        {
            // Move along the local forward axis
            projectile.transform.Translate(Vector3.forward * (Speed * Time.deltaTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Destroy the projectile after its lifetime
        Destroy(projectile);
    }
}
