using System.Collections.Generic;
using UnityEngine;

public class S_EnergyPointPoolManager : MonoBehaviour
{
    public static S_EnergyPointPoolManager Instance;

    [Header("Spawn Rate Limiting")]
    public int maxActivationsPerFrame = 5;

    private Dictionary<GameObject, Queue<GameObject>> pool = new();
    private Queue<Request> spawnQueue = new();

    private struct Request
    {
        public GameObject prefab;
        public Vector3 position;
        public Vector3 direction;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        int count = Mathf.Min(maxActivationsPerFrame, spawnQueue.Count);
        for (int i = 0; i < count; i++)
        {
            Request req = spawnQueue.Dequeue();
            ProcessSpawn(req);
        }
    }

    public void QueueEnergyPoint(GameObject prefab, Vector3 position, Vector3 direction)
    {
        spawnQueue.Enqueue(new Request
        {
            prefab = prefab,
            position = position,
            direction = direction
        });
    }

    private void ProcessSpawn(Request req)
    {
        if (!pool.TryGetValue(req.prefab, out var q))
            pool[req.prefab] = q = new Queue<GameObject>();

        GameObject obj = null;

        while (q.Count > 0 && (obj = q.Dequeue()) == null) { }

        if (obj == null)
            obj = Instantiate(req.prefab);
        else
            obj.SetActive(true);

        Transform t = obj.transform;
        t.position  = req.position;
        t.rotation  = Quaternion.identity;

        if (obj.TryGetComponent(out S_AddEnergyTypeWithDelay energySetting))
            energySetting.selfPrefab = req.prefab;

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(req.direction * 10f, ForceMode.Impulse);
        }
    }


    public void ReturnToPool(GameObject obj, GameObject prefab)
    {
        if (obj == null) return;
        obj.SetActive(false);
        Destroy(obj.GetComponent<EnergyType>());
        if (!pool.ContainsKey(prefab))
            pool[prefab] = new Queue<GameObject>();

        pool[prefab].Enqueue(obj);
    }
}
