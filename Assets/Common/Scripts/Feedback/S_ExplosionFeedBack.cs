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
    public float Fade_Multiplier = 2f;
    private bool timer_work;
    private float timer_groundpound;
    public float Min_Fade = 300f;
    private float Fade_Distance = 300f;

    private void Start()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiceGroudPoundEvevent;
        }

        Fade_Distance = Min_Fade;
    }

    private void Update()
    {
        if (In_Ground)
        {
            ShockwaveMaterial.SetFloat("_Distance", distanceShockwave);
        }

        if (timer_work)
        {
            timer_groundpound += Time.deltaTime;
            distanceShockwave += Time.deltaTime * Shock_Wave_Speed_Multiplayer;
            Fade_Distance += Time.deltaTime * Fade_Multiplier;
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
        timer_groundpound = 0f;
        Shock_Wave_Speed_Multiplayer = 40f;
        ParticleSystem Onde = Instantiate(OndeParticule, impactPosition, ImpactShockWave.transform.rotation);
        Onde.Play();
        GameObject ShockWave = Instantiate(ImpactShockWave, impactPosition, ImpactShockWave.transform.rotation);
        ShockwaveMaterial = ShockWave.GetComponent<Renderer>().material;
        In_Ground = true;
        timer_work = true;
        ShockwaveMaterial.SetFloat("_Max_FadeDistance", Fade_Distance);
        Destroy(ShockWave, 3f);
        Destroy(Onde,2f);
        Fade_Distance = Min_Fade;
    }
}