using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_oldEnergyStorage : MonoBehaviour
{
    [Header("Energy Settings")]
    public bool isUnlimitedStorage = false; // If true, energy storage is unlimited
    public float maxEnergy = 100f; // Maximum energy capacity (if not unlimited)
    public float currentEnergy = 0f; // Current stored energy

    [Header("Detection Settings")]
    public float detectionRadius = 5f; // Radius of the sphere detection
    public string targetScriptName; // Name of the script to detect on objects
    public float energyGainPerObject = 10f; // Energy gained per absorbed object

    [Header("Player Settings")]
    public float pullSpeed = 5f; // Speed at which objects are pulled towards the player

    [Header("UI Settings")]
    public TextMeshProUGUI energyDisplay; // TextMeshPro to display energy value

    private Transform playerTransform; // The player's transform
    private HashSet<GameObject> pullingObjects = new HashSet<GameObject>(); // Track objects being pulled

    private void Start()
    {
        playerTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        DetectAndPullObjects();
        UpdateEnergyDisplay();
    }

    private void DetectAndPullObjects()
    {
        // Stop detecting objects if max energy is reached
        if (!isUnlimitedStorage && currentEnergy >= maxEnergy)
        {
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in hitColliders)
        {
            if (pullingObjects.Contains(collider.gameObject))
            {
                continue; // Skip already pulling objects
            }

            Component targetScript = collider.GetComponent(targetScriptName);
            if (targetScript != null)
            {
                pullingObjects.Add(collider.gameObject);
                StartCoroutine(PullAndDestroyObject(collider.gameObject));
            }
        }
    }

    private IEnumerator PullAndDestroyObject(GameObject obj)
    {
        while (obj != null && Vector3.Distance(obj.transform.position, playerTransform.position) > 1.5f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, playerTransform.position, pullSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj != null) // Ensure the object still exists before destroying
        {
            pullingObjects.Remove(obj); // Ensure it's removed from tracking before destruction
            Destroy(obj);
            AddEnergy(energyGainPerObject);
        }
    }

    private void AddEnergy(float amount)
    {
        if (isUnlimitedStorage)
        {
            currentEnergy += amount;
        }
        else
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        }
    }

    private void UpdateEnergyDisplay()
    {
        if (energyDisplay != null)
        {
            energyDisplay.text = isUnlimitedStorage ? $"Energy: {currentEnergy:F2}" : $"Energy: {currentEnergy:F2}/{maxEnergy}";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
