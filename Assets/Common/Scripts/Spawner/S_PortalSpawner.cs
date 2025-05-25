using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns and scales a portal based on S_SpawnZone.weight.
/// </summary>
public class S_PortalSpawner : MonoBehaviour
{
    [Header("Portal Settings")]
    public GameObject portalPrefab;
    public Vector3 spawnOffset = Vector3.zero;
    public float openThreshold = 0.8f;
    public float closeThreshold = 0.2f;

    [Header("Scale Animation Settings")]
    public AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float scaleUpDuration = 0.5f;
    public AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float scaleDownDuration = 0.3f;

    [Header("Close Condition")]
    public float readyToCloseCheckThreshold = 0.98f;

    private S_SpawnZone spawnZone;
    private GameObject currentPortal;
    private Coroutine scaleCoroutine;
    private bool portalReadyToClose;
    private bool portalFullySpawned;
    private Vector3 originalScale = Vector3.one;

    void Start()
    {
        spawnZone = GetComponent<S_SpawnZone>();
        if (portalPrefab != null)
            originalScale = portalPrefab.transform.localScale;
    }

    void Update()
    {
        if (spawnZone == null || portalPrefab == null)
            return;

        float w = spawnZone.weight;

        // spawn portal
        if (w > openThreshold && currentPortal == null)
            SpawnPortal();

        // allow close check
        if (portalFullySpawned && !portalReadyToClose && w >= readyToCloseCheckThreshold)
            portalReadyToClose = true;

        // close portal
        if (currentPortal != null && portalFullySpawned && portalReadyToClose && w < closeThreshold)
            StartClose();
    }

    void SpawnPortal()
    {
        Vector3 pos = transform.position + spawnOffset;
        currentPortal = Instantiate(portalPrefab, pos, Quaternion.identity);
        currentPortal.transform.localScale = Vector3.zero;
        scaleCoroutine = StartCoroutine(ScalePortal(currentPortal.transform, originalScale, false));
    }

    void StartClose()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScalePortal(currentPortal.transform, Vector3.zero, true));
    }

    // scales portal up or down, then resets or destroys
    IEnumerator ScalePortal(Transform tf, Vector3 target, bool destroy)
    {
        Vector3 start = tf.localScale;
        float dur = destroy ? scaleDownDuration : scaleUpDuration;
        AnimationCurve curve = destroy ? scaleDownCurve : scaleUpCurve;
        float t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime;
            float v = curve.Evaluate(t / dur);
            tf.localScale = Vector3.LerpUnclamped(start, target, v);
            yield return null;
        }

        tf.localScale = target;

        if (destroy)
        {
            Destroy(tf.gameObject);
            currentPortal = null;
            portalFullySpawned = false;
            portalReadyToClose = false;
        }
        else
        {
            portalFullySpawned = true;
            portalReadyToClose = false;
        }
    }
}
