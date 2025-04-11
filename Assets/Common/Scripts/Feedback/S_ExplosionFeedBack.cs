using System;
using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [Header("Onde_GroundPound")] 
    public ParticleSystem OndeParticule;
    
    [Header("ShockWave")]

    public GameObject ShockWavePoint;
    public GameObject ImpactShockWave;
    private float distanceShockwave;
    private bool In_Ground;
    private Material ShockwaveMaterial;
    public float Shock_Wave_Speed_Multiplayer = 1f;

    private void Start()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiceGroudPoundEvevent;
        }
    }

    private void Update()
    {
        if (In_Ground)
        {
            distanceShockwave += Time.deltaTime * Shock_Wave_Speed_Multiplayer;
            ShockwaveMaterial.SetFloat("_Distance", distanceShockwave);
            float amplitude = Mathf.Lerp(10f, 0f, distanceShockwave / 200f);
            ShockwaveMaterial.SetFloat("_Amplitude", amplitude);
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
        distanceShockwave = 8f;
        ParticleSystem Onde = Instantiate(OndeParticule, impactPosition, ImpactShockWave.transform.rotation);
        Onde.Play();
        GameObject ShockWave = Instantiate(ImpactShockWave, impactPosition, ImpactShockWave.transform.rotation);
        ShockwaveMaterial = ShockWave.GetComponent<Renderer>().material;
        In_Ground = true;
        Destroy(ShockWave, 3f);
        Destroy(Onde,2f);
    }
}