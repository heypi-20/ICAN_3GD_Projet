using System;
using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Onde_GroundPound")]

    
    [Header("ShockWave")]

    private float currentDistance = 0f;
    private bool isPlaying = false;
    public GameObject ShockWavePoint;
    public Material shockwaveMat;
    public GameObject ImpactShockWave;

    private void Start()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiceGroudPoundEvevent;
        }
    }
    

    private void ReceiceGroudPoundEvevent(Enum state)
    {
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            SpawnExplosion_GroundPound(ShockWavePoint.transform.position);
        }
    }
    private void SpawnExplosion_GroundPound(Vector3 impactPosition)
    {
        float currentTime = Time.time;
        GameObject ShockWave = Instantiate(ImpactShockWave, impactPosition, ImpactShockWave.transform.rotation);
        Destroy(ShockWave, 0.5f);
    }
    
    
}