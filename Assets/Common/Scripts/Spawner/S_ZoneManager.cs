using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class S_ZoneManager : MonoBehaviour
{
    public static S_ZoneManager Instance { get; private set; }
    
    [Header("Zone manager settings")]
    public float trackInterval = 0.5f;
    public float weightThreshold = 0.5f;
    public float timeToBeReactive = 0.5f;
    
        
    [Header("Automatic search for references")]
    public List<S_SpawnZone> spawnZones;
    public GameObject player;
    
    private Vector3 playerPosition;
    private List<S_SpawnZone> ActiveZonesByPlayer;
    private HashSet<S_SpawnZone> zonesBeingWatched = new HashSet<S_SpawnZone>();
    


    private void Awake()
    {
        SimpleSingleton();
        // Get all instances of S_SpawnZone in the scene and store them in spawnZones
        spawnZones = new List<S_SpawnZone>(FindObjectsOfType<S_SpawnZone>());
        // Get the GameObject with the S_CustomCharacterController component
        player = FindObjectOfType<S_CustomCharacterController>().gameObject;
        ActiveZonesByPlayer = new List<S_SpawnZone>();

    }

    private void Start()
    {
        // Start continuous tracking of player position using a coroutine
        StartCoroutine(TrackPlayerRoutine());
    }
    
    private void SimpleSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ZoneManager] Duplicate instance found, destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator TrackPlayerRoutine()
    {
        while (true)
        {
            HandlePlayerEntry();
            yield return new WaitForSeconds(trackInterval);
        }
    }

    private void Update()
    {
        HandlePlayerEntry();
        HandleActivatedSpawnZoneByPlayer();
    }
    private void HandlePlayerEntry()
    {
        // Update player's current position
        playerPosition = player.transform.position;

        foreach (S_SpawnZone zone in spawnZones)
        {
            float distance = Vector3.Distance(playerPosition, zone.transform.position);
            if (distance <= zone.radius&&!ActiveZonesByPlayer.Contains(zone))
            {
                zone.StartWeightLossTimer();
                ActiveZonesByPlayer.Add(zone);
                Debug.Log(zone.name + " is active by the player");
            }
            
            if (zone.weight == 0 && distance > zone.radius && ActiveZonesByPlayer.Contains(zone) && !zone.isWaitingToBeReactivated)
            {
                zone.isWaitingToBeReactivated = true;
                Debug.Log("Start Zone coroutine");
                StartCoroutine(WaitForXSecToBeReactive(zone));
            }
        }
    }

    private IEnumerator WaitForXSecToBeReactive(S_SpawnZone zone)
    {
        yield return new WaitForSeconds(timeToBeReactive);
        zone.alreadyTriggerAnotherZone = false;
        ActiveZonesByPlayer.Remove(zone);
        zone.isWaitingToBeReactivated = false;
        Debug.Log(zone.name + " is now removed and ready to be reactivated.");
    }


    private void HandleActivatedSpawnZoneByPlayer()
    {
        if (ActiveZonesByPlayer == null) return;

        foreach (S_SpawnZone playerZone in ActiveZonesByPlayer)
        {
            if (playerZone != null && playerZone.weight <= weightThreshold&&!playerZone.alreadyTriggerAnotherZone)
            {
                S_SpawnZone nearestZone = null;
                float minDistance = Mathf.Infinity;

                foreach (S_SpawnZone zone in spawnZones)
                {
                    if (zone.weight == 0 && !ActiveZonesByPlayer.Contains(zone))
                    {
                        float distance = Vector3.Distance(playerZone.transform.position, zone.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestZone = zone;
                        }
                    }
                }
                if (nearestZone != null)
                {
                    nearestZone.StartWeightGainTimer();
                    playerZone.alreadyTriggerAnotherZone = true;
                    if (!zonesBeingWatched.Contains(nearestZone))
                    {
                        StartCoroutine(WatchZoneUntilFullyCharged(nearestZone));
                        zonesBeingWatched.Add(nearestZone);
                    }
                    Debug.Log(nearestZone.name + " triggered by " + playerZone.name);
                    break;
                }
            }
        }
    }
    
    private IEnumerator WatchZoneUntilFullyCharged(S_SpawnZone zone)
    {
        // Wait until the zone is fully charged (weight >= 1)
        while (zone.weight < 1f)
        {
            yield return null;
        }

        // Wait an extra frame to ensure stability
        yield return null;

        // Trigger as if player entered it (but only if player didn't already do so)
        if (!ActiveZonesByPlayer.Contains(zone))
        {
            zone.StartWeightLossTimer();
            ActiveZonesByPlayer.Add(zone);
            Debug.Log(zone.name + " reached full weight and is now activated by system");
        }

        // Done watching
        zonesBeingWatched.Remove(zone);
    }

    public Vector3[] GetSpawnPointsByEnemyWithZoneWeight(EnemyType type)
    {
        foreach (S_SpawnZone zone in spawnZones)
        {
            Vector3[] points = zone.GetSpawnPointsByEnemyType(type);
            if (points == null || points.Length == 0)
                continue;
            if (Mathf.Approximately(zone.weight, 1f))
            {
                return points;
            }
            
            float chance = Random.Range(0f, 1f);
            if (chance <= zone.weight)
            {
                return points;  
            }
        }
        return null;

    }
    
    

}
