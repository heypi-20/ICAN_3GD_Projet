using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnergyAbsorption_Module : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 5f;             // Detection radius for the sphere
    [Tooltip("Max number of colliders to check per frame")]
    public int maxDetectedObjects = 20;            // Max number of objects per detection
    public float pullSpeed = 5f;                   // Speed at which objects are pulled to the player

    // Buffer for non-alloc overlap queries
    private Collider[] detectionBuffer;

    // References for energy & combo
    private Transform _playerTransform;
    private HashSet<GameObject> _pullingObjects = new HashSet<GameObject>();
    private S_EnergyStorage _energyStorage;
    private S_ComboSystem _comboSystem;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Only in editor: keep buffer size synced with maxDetectedObjects
        if (maxDetectedObjects > 0)
            detectionBuffer = new Collider[maxDetectedObjects];
    }
    #endif

    private void Awake()
    {
        // Ensure buffer is initialized at runtime
        if (detectionBuffer == null || detectionBuffer.Length != maxDetectedObjects)
            detectionBuffer = new Collider[maxDetectedObjects];
    }

    private void Start()
    {
        // Component lookups
        _playerTransform = transform;
        _energyStorage   = GetComponent<S_EnergyStorage>();
        _comboSystem     = GetComponent<S_ComboSystem>();
    }

    private void Update()
    {
        DetectAndPullObjects();
    }

    private void DetectAndPullObjects()
    {
        // If at max energy, skip detection
        if (_energyStorage.currentEnergy >= _energyStorage.maxEnergy)
            return;

        // Non-alloc overlap query
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            detectionBuffer
        );

        if (hitCount <= 0)
            return;

        // Sort first hitCount entries by ascending distance to player
        Array.Sort(
            detectionBuffer,
            0,
            hitCount,
            new ColliderDistanceComparer(transform.position)
        );

        // Pull each valid energy object, nearest first
        for (int i = 0; i < hitCount; i++)
        {
            var collider = detectionBuffer[i];
            if (_pullingObjects.Contains(collider.gameObject))
                continue;

            EnergyType energyType = collider.GetComponent<EnergyType>();
            if (energyType == null)
                continue;

            // Begin pulling the nearest one
            _pullingObjects.Add(collider.gameObject);
            SoundManager.Instance.Meth_Gain_Energy();
            StartCoroutine(
                PullAndDestroyObject(
                    collider.gameObject,
                    energyType.energyGivenUseForType
                )
            );
        }
    }

    private IEnumerator PullAndDestroyObject(GameObject obj, float givenPoint)
    {
        while (obj != null)
        {
            if (Vector3.Distance(obj.transform.position, _playerTransform.position) <= 2f)
                break;

            // Move object towards player
            obj.transform.position = Vector3.MoveTowards(
                obj.transform.position,
                _playerTransform.position,
                pullSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (obj != null)
        {
            _pullingObjects.Remove(obj);
            _energyStorage.AddEnergy(
                givenPoint * _comboSystem.currentComboMultiplier
            );

            if (obj.TryGetComponent(out S_AddEnergyTypeWithDelay energySetting))
            {
                S_EnergyPointPoolManager.Instance.ReturnToPool(
                    obj,
                    energySetting.selfPrefab
                );
            }
            else
            {
                Destroy(obj);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    // Comparer that orders Colliders by distance squared to a given origin
    private class ColliderDistanceComparer : IComparer<Collider>
    {
        private readonly Vector3 origin;
        public ColliderDistanceComparer(Vector3 origin)
        {
            this.origin = origin;
        }

        public int Compare(Collider a, Collider b)
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        }
    }
}
