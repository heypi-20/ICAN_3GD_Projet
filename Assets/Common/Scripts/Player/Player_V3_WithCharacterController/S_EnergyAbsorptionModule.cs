using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class S_EnergyAbsorptionModule : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 5f; // Radius of the sphere detection
    public float pullSpeed = 5f; // Speed at which objects are pulled towards the player
    private Transform _playerTransform; // The player's transform
    private HashSet<GameObject> _pullingObjects = new HashSet<GameObject>(); // Track objects being pulled
    private S_EnergyStorage _energyStorage;
    private void Start()
    {
        _playerTransform = GetComponent<Transform>();
        _energyStorage = GetComponent<S_EnergyStorage>();
    }
    
    private void Update()
    {
        DetectAndPullObjects();
    }
    private void DetectAndPullObjects()
    {
        // Stop detecting objects if max energy is reached
        if (_energyStorage.currentEnergy >= _energyStorage.maxEnergy)
        {
            return;
        }
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (Collider collider in hitColliders)
        {
            if (_pullingObjects.Contains(collider.gameObject))
            {
                continue; // Skip already pulling objects
            }

            EnergyType energyType = collider.GetComponent<EnergyType>();
            if (energyType is not null)
            {
                _pullingObjects.Add(collider.gameObject);
                StartCoroutine(PullAndDestroyObject(collider.gameObject,energyType.energyGiven));
            }
        }
    }

    private IEnumerator PullAndDestroyObject(GameObject obj,float givenPoint)
    {
        while (obj is not null && Vector3.Distance(obj.transform.position, _playerTransform.position) > 2f)
        {
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, _playerTransform.position, pullSpeed * Time.deltaTime);
            yield return null;
        }

        if (obj is not null) // Ensure the object still exists before destroying
        {
            _pullingObjects.Remove(obj); // Ensure it's removed from tracking before destruction
            _energyStorage.AddEnergy(givenPoint);
            Destroy(obj);
           
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
