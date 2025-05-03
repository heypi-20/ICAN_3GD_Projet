using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_EnergyAbsorption_Module : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRadius = 5f;                 // Detection radius for the sphere
    [Tooltip("Max number of colliders to check per frame")]
    public int maxDetectedObjects = 20;                // Max number of objects per detection
    [Tooltip("Which layers contain energy objects")]
    public LayerMask energyLayerMask;                  // Only detect colliders on these layers
    public float pullSpeed = 5f;                       // Speed at which objects are pulled to the player

    // Buffer for non-alloc overlap queries
    private Collider[] detectionBuffer;
    // Temporary list to hold valid hits each frame
    private List<Collider> validHits;

    // References for player, energy storage & combo system
    private Transform _playerTransform;
    private S_EnergyStorage _energyStorage;
    private S_ComboSystem _comboSystem;

    // Track which objects are already being pulled
    private HashSet<GameObject> _pullingObjects = new HashSet<GameObject>();

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Only in editor: keep buffer size in sync with maxDetectedObjects
        if (maxDetectedObjects > 0)
            detectionBuffer = new Collider[maxDetectedObjects];
    }
    #endif

    private void Awake()
    {
        // Ensure buffer and list are initialized at runtime
        if (detectionBuffer == null || detectionBuffer.Length != maxDetectedObjects)
            detectionBuffer = new Collider[maxDetectedObjects];
        validHits = new List<Collider>(maxDetectedObjects);
    }

    private void Start()
    {
        // Cache references
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
        // If at max energy, skip detection entirely
        if (_energyStorage.currentEnergy >= _energyStorage.maxEnergy)
            return;

        // Perform non-alloc overlap query, only colliders on energyLayerMask and triggers
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            detectionRadius,
            detectionBuffer,
            energyLayerMask,
            QueryTriggerInteraction.Collide
        );

        if (hitCount <= 0)
            return;

        // Build a list of truly valid, active hits this frame
        validHits.Clear();
        for (int i = 0; i < hitCount; i++)
        {
            var col = detectionBuffer[i];

            // Skip null slots or pooled (inactive) objects
            if (col == null || !col.gameObject.activeInHierarchy)
                continue;

            var go = col.gameObject;

            // Skip objects already being pulled
            if (_pullingObjects.Contains(go))
                continue;

            // Verify this object actually has an EnergyType
            if (go.GetComponent<EnergyType>() == null)
                continue;

            validHits.Add(col);
        }

        if (validHits.Count == 0)
            return;

        // Sort by distance (closest first)
        validHits.Sort((a, b) =>
            ((a.transform.position - transform.position).sqrMagnitude)
            .CompareTo(
            (b.transform.position - transform.position).sqrMagnitude
        ));

        // Pull each object in order
        foreach (var col in validHits)
        {
            var go = col.gameObject;
            var energyType = go.GetComponent<EnergyType>();

            Debug.Log($"[Detect] Pulling {go.name} at distance {(go.transform.position - transform.position).magnitude:F2}");

            _pullingObjects.Add(go);
            SoundManager.Instance.Meth_Gain_Energy();
            StartCoroutine(PullAndDestroyObject(go, energyType.energyGivenUseForType));
        }
    }

    private IEnumerator PullAndDestroyObject(GameObject obj, float givenPoint)
    {
        // Debug the calculated energy amount
        float multiplier = _comboSystem.currentComboMultiplier;
        float amount = givenPoint * multiplier;
        Debug.Log($"[Energy] givenPoint={givenPoint}, multiplier={multiplier}, toAdd={amount}");

        // Pull object towards player until close enough
        while (obj != null)
        {
            if (Vector3.Distance(obj.transform.position, _playerTransform.position) <= 2f)
                break;

            obj.transform.position = Vector3.MoveTowards(
                obj.transform.position,
                _playerTransform.position,
                pullSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (obj != null)
        {
            // Remove from pulling set and add energy
            _pullingObjects.Remove(obj);
            _energyStorage.AddEnergy(amount);
            Debug.Log($"[Energy] After AddEnergy: {_energyStorage.currentEnergy}/{_energyStorage.maxEnergy}");

            // Return to pool if support component exists, otherwise destroy
            if (obj.TryGetComponent(out S_AddEnergyTypeWithDelay energySetting))
                S_EnergyPointPoolManager.Instance.ReturnToPool(obj, energySetting.selfPrefab);
            else
                Destroy(obj);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize detection radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
