using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ZoneManager : MonoBehaviour
{
    // Singleton instance of S_ZoneManager.
    public static S_ZoneManager Instance { get; private set; }
    
    // Zone manager settings.
    public float trackInterval = 0.5f;      // How often to check the player's position.
    public float weightThreshold = 0.5f;    // Weight value threshold to trigger chain activation.
    public float timeToBeReactive = 0.5f;   // Time to wait before a zone can be reactivated.
    
    // References found automatically.
    public List<S_SpawnZone> spawnZones;    // List of all spawn zones in the scene.
    public GameObject player;               // Reference to the player GameObject.
    
    private Vector3 playerPosition;         // Current player position.
    private List<S_SpawnZone> ActiveZonesByPlayer; // List of zones currently active due to player proximity.
    private HashSet<S_SpawnZone> zonesBeingWatched = new HashSet<S_SpawnZone>(); // Zones being monitored for full charge.
    
    private void Awake()
    {
        // Setup singleton pattern.
        SimpleSingleton();
        
        // Get all S_SpawnZone components in the scene.
        spawnZones = new List<S_SpawnZone>(FindObjectsOfType<S_SpawnZone>());
        
        // Find the player by looking for the S_CustomCharacterController component.
        player = FindObjectOfType<S_CustomCharacterController>().gameObject;
        
        ActiveZonesByPlayer = new List<S_SpawnZone>();
    }

    private void Start()
    {
        // Start continuously tracking the player's position.
        StartCoroutine(TrackPlayerRoutine());
    }
    
    // Simple singleton setup to ensure only one instance exists.
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

    // Continuously checks the player's position at set intervals.
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
        // Continuously handle player entry and update active zones.
        HandlePlayerEntry();
        HandleActivatedSpawnZoneByPlayer();
    }
    
    // Checks if the player has entered or left a zone.
    private void HandlePlayerEntry()
    {
        // Update the player's current position.
        playerPosition = player.transform.position;

        foreach (S_SpawnZone zone in spawnZones)
        {
            float distance = Vector3.Distance(playerPosition, zone.transform.position);
            
            // If the player is within the zone's radius and the zone is not already active.
            if (distance <= zone.radius && !ActiveZonesByPlayer.Contains(zone))
            {
                zone.StartWeightLossTimer();
                ActiveZonesByPlayer.Add(zone);
                Debug.Log(zone.name + " is active by the player");
            }
            
            // If the zone's weight is 0 and the player is outside the zone, mark it for reactivation.
            if (zone.weight == 0 && distance > zone.radius && ActiveZonesByPlayer.Contains(zone) && !zone.isWaitingToBeReactivated)
            {
                zone.isWaitingToBeReactivated = true;
                Debug.Log("Start Zone coroutine");
                StartCoroutine(WaitForXSecToBeReactive(zone));
            }
        }
    }

    // Waits for a set time before allowing a zone to be reactivated.
    private IEnumerator WaitForXSecToBeReactive(S_SpawnZone zone)
    {
        yield return new WaitForSeconds(timeToBeReactive);
        zone.alreadyTriggerAnotherZone = false;
        ActiveZonesByPlayer.Remove(zone);
        zone.isWaitingToBeReactivated = false;
        Debug.Log(zone.name + " is now removed and ready to be reactivated.");
    }

    // Checks active zones and triggers nearby zones based on weight.
    private void HandleActivatedSpawnZoneByPlayer()
    {
        if (ActiveZonesByPlayer == null) return;

        foreach (S_SpawnZone playerZone in ActiveZonesByPlayer)
        {
            // If the active zone's weight is below the threshold and hasn't triggered another zone.
            if (playerZone != null && playerZone.weight <= weightThreshold && !playerZone.alreadyTriggerAnotherZone)
            {
                S_SpawnZone nearestZone = null;
                float minDistance = Mathf.Infinity;

                // Find the nearest inactive zone (weight is 0) that is not already active.
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
                    // Start watching the zone until it is fully charged.
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
    
    // Monitors a zone until its weight reaches full charge (1).
    private IEnumerator WatchZoneUntilFullyCharged(S_SpawnZone zone)
    {
        // Wait until the zone's weight reaches or exceeds 1.
        while (zone.weight < 1f)
        {
            yield return null;
        }
        // Wait one extra frame to ensure stability.
        yield return null;
        // If the zone is not already active, start its weight loss timer.
        if (!ActiveZonesByPlayer.Contains(zone))
        {
            zone.StartWeightLossTimer();
            ActiveZonesByPlayer.Add(zone);
            Debug.Log(zone.name + " reached full weight and is now activated by system");
        }
        // Stop watching this zone.
        zonesBeingWatched.Remove(zone);
    }

    // Returns spawn points for an enemy based on the zone's weight and a random chance.
    public Vector3[] GetSpawnPointsByEnemyWithZoneWeight(EnemyType type)
    {
        foreach (S_SpawnZone zone in spawnZones)
        {
            Vector3[] points = zone.GetSpawnPointsByEnemyType(type);
            if (points == null || points.Length == 0)
                continue;
            // If the zone weight is exactly 1, return its points.
            if (Mathf.Approximately(zone.weight, 1f))
            {
                return points;
            }
            // Otherwise, return the points based on a random chance compared to the zone's weight.
            float chance = Random.Range(0f, 1f);
            if (chance <= zone.weight)
            {
                return points;  
            }
        }
        return null;
    }
}
