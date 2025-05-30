using System;
using System.Collections;
using UnityEngine;

public class BossPhaseActivator : MonoBehaviour
{
    [Header("Delay Settings (seconds)")]
    public float eggDelay = 3f;     // Time to wait before activating the boss egg
    public float bossDelay = 5f;    // Time to wait before activating the boss itself

    [Header("Phase Objects")]
    public GameObject normalPhaseUI;           // UI for the normal phase
    public GameObject normalPhaseSpawner;      // Spawner used in the normal phase
    public GameObject bossPhaseUI;             // UI for the boss phase
    public GameObject bossPhaseSpawnerPrefab;  // Prefab to instantiate for boss phase spawning
    public GameObject bossEgg;                 // The egg object appearing before the boss
    public GameObject bossObject;              // The actual boss object
    
    [Header("Knockback Settings")]
    public float knockbackRadius;
    public float knockbackForce;
    public float knockbackDuration;
    public LayerMask playerLayer;

    /// <summary>
    /// Call this method to start the boss phase sequence.
    /// </summary>
    public void StartBossPhase()
    {
        // Destroy the normal phase spawner
        StartCoroutine(BossPhaseSequence());
    }

    private IEnumerator BossPhaseSequence()
    {
        // 1) Wait for eggDelay, then enable the boss egg
        yield return new WaitForSeconds(eggDelay);
        if (bossEgg != null)
            bossEgg.SetActive(true);

        if (bossEgg.activeSelf) {
            Collider[] hits = Physics.OverlapSphere(bossEgg.transform.position, knockbackRadius, playerLayer);

            foreach (Collider hit in hits) {
                Debug.Log("Start Knockback player");
                Debug.Log(hit.gameObject.name);
    
                CharacterController controller = hit.GetComponent<CharacterController>();
                if (controller != null) {
                    Vector3 dir = (hit.transform.position - bossEgg.transform.position).normalized;
                    StartCoroutine(ApplyKnockback(controller, dir));
                }
            }
        }
        
        // 2) Wait for bossDelay, then enable boss, switch UI, destroy normal spawner and spawn boss spawner
        yield return new WaitForSeconds(bossDelay);

        // Activate the boss
        if (bossObject != null)
            bossObject.SetActive(true);

        // Switch UIs
        if (normalPhaseUI != null) normalPhaseUI.SetActive(false);
        if (bossPhaseUI   != null) bossPhaseUI.SetActive(true);

        if (normalPhaseSpawner != null)
            Destroy(normalPhaseSpawner);
        yield return new WaitForSeconds(1);

        // Instantiate the boss phase spawner prefab at this object's position
        if (bossPhaseSpawnerPrefab != null)
            Instantiate(bossPhaseSpawnerPrefab, transform.position, Quaternion.identity);
    }

    private IEnumerator ApplyKnockback(CharacterController controller, Vector3 dir)
    {
        float timer = 0f;
        
        while (timer < knockbackDuration) {
            controller.Move(dir * knockbackForce * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
