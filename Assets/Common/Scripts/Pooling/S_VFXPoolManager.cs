using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_VFXPoolManager : MonoBehaviour
{
    public static S_VFXPoolManager Instance;
    public float VFXStayTime;

    private Dictionary<GameObject, Queue<GameObject>> pool = new();
    private Queue<Request> vfxRequestQueue = new(); 

    [Tooltip("How many VFX to activate per frame")]
    public int maxActivationsPerFrame = 50;

    private class Request
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public float releaseTime;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void SpawnVFX(GameObject prefab, Vector3 position, Quaternion rotation, float autoReleaseTime = 3f)
    {
        vfxRequestQueue.Enqueue(new Request
        {
            prefab = prefab,
            position = position,
            rotation = rotation,
            releaseTime = autoReleaseTime
        });
    }

    private void Update()
    {
        int count = Mathf.Min(maxActivationsPerFrame, vfxRequestQueue.Count);
        for (int i = 0; i < count; i++)
        {
            Request request = vfxRequestQueue.Dequeue();
            ProcessRequest(request);
        }
    }

    private void ProcessRequest(Request req)
    {
        if (!pool.ContainsKey(req.prefab))
            pool[req.prefab] = new Queue<GameObject>();

        GameObject vfx;
        if (pool[req.prefab].Count > 0)
        {
            vfx = pool[req.prefab].Dequeue();
            vfx.transform.position = req.position;
            vfx.transform.rotation = req.rotation;
            vfx.SetActive(true);
        }
        else
        {
            vfx = Instantiate(req.prefab, req.position, req.rotation);
        }

        StartCoroutine(ReleaseAfter(vfx, req.prefab, VFXStayTime));
    }

    private IEnumerator ReleaseAfter(GameObject obj, GameObject prefab, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
        pool[prefab].Enqueue(obj);
    }
}
