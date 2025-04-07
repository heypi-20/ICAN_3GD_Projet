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
    private Material myMaterial;
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
            myMaterial = ImpactShockWave.GetComponent<Renderer>().sharedMaterial;
            float dist = myMaterial.GetFloat("Dist");
            Debug.Log("Valeur du shader Dist : " + dist);
            dist = 2f;
            dist = 2 + (Time.deltaTime * Shock_Wave_Speed_Multiplayer);
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
        ParticleSystem Onde = Instantiate(OndeParticule, impactPosition, ImpactShockWave.transform.rotation);
        Onde.Play();
        GameObject ShockWave = Instantiate(ImpactShockWave, impactPosition, ImpactShockWave.transform.rotation);
        In_Ground = true;
        Destroy(ShockWave, 0.5f);
        Destroy(Onde,2f);
    }
}