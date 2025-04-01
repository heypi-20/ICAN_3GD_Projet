using System;
using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [Header("GroundPoundExplosion")]
    public GameObject explosionPrefab;  // Le prefab de l'explosion
    private GameObject currentExplosion; // Stocke l'objet instancié

    public AnimationCurve sizeCurve;
    public AnimationCurve alphaCurve;
    public float duration = 1f;
    public MeshRenderer sphereRenderer;  // Renseigner depuis l'inspecteur

    private Material sphereMaterial;
    public GameObject Explosion_GroundPound_SpawnPoint;
    
    [Header("ShockWave")]
    public Material shockwaveMaterial;
    public float shockwaveSpeed = 10f;
    public float maxDistance = 20f;
    public float intensity = 10f;
    public ParticleSystem ExplosionParticule;
    private float shape_responsive;

    private float currentDistance = 0f;
    private bool isPlaying = false;
    public GameObject ShockWavePoint;
    public Material shockwaveMat;
    public GameObject ImpactShockWave;
    
    
    private void OnEnable()
    {
        if (S_PlayerStateObserver.Instance != null)
        {
            S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiGroudPoundEvevent;
        }
        else
        {
            StartCoroutine(WaitForObserver());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Play(ShockWavePoint.transform.position);
        }
        if (isPlaying)
        {
            currentDistance += Time.deltaTime * shockwaveSpeed;

            // Envoie les valeurs au shader
            shockwaveMaterial.SetVector("_Shockwave_Position", ShockWavePoint.transform.position);
            shockwaveMaterial.SetFloat("_Shockwave_Distance", currentDistance);
            shockwaveMaterial.SetFloat("_Shockwave_Intensity", Mathf.Lerp(intensity, 0f, currentDistance / maxDistance));

            // Stopper une fois terminé
            if (currentDistance >= maxDistance)
            {
                isPlaying = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float currentTime = Time.time;
            shockwaveMat.SetFloat("_StartTime", currentTime);
        }
    }

    private IEnumerator WaitForObserver()
    {
        float timeout = 3f;
        float elapsedTime = 0f;

        while (S_PlayerStateObserver.Instance == null)
        {
            if (elapsedTime >= timeout)
            {
                //Debug.LogError("S_PlayerStateObserver.Instance not found after waiting " + timeout + " seconds.");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        S_PlayerStateObserver.Instance.OnGroundPoundStateEvent += ReceiGroudPoundEvevent;
    }

    private void ReceiGroudPoundEvevent(Enum state)
    {
        if (state.Equals(PlayerStates.GroundPoundState.EndGroundPound))
        {
            //Debug.Log("Ca devrait exploser ici");
            //TriggerExplosion(Explosion_GroundPound_SpawnPoint.transform.position);
            SpawnParticule(Explosion_GroundPound_SpawnPoint.transform.position);
        }
    }
    


    private void TriggerExplosion(Vector3 impactPosition)
    {
        // Instancie l'explosion à la position d'impact
        currentExplosion = Instantiate(explosionPrefab, impactPosition, Quaternion.identity);

        // Récupère le MeshRenderer sur l'objet instancié
        sphereRenderer = currentExplosion.GetComponentInChildren<MeshRenderer>();

        if (sphereRenderer != null)
        {
            sphereMaterial = sphereRenderer.material;
            StartCoroutine(ExplosionRoutine());
        }
    }

    private void SpawnParticule(Vector3 impactPosition)
    {
        S_GroundPound_Module groundPoundModule = FindObjectOfType<S_GroundPound_Module>();
        float range = groundPoundModule.DynamicSphereRange;
        
        var main = ExplosionParticule.main;
        main.startSize = range * 4f; // Exemple : vitesse proportionnelle au range
        
        ParticleSystem newParticles = Instantiate(ExplosionParticule, impactPosition, ExplosionParticule.transform.rotation);
        GameObject ShockWave = Instantiate(ImpactShockWave, impactPosition, ImpactShockWave.transform.rotation);
        newParticles.Play();
        Destroy(ShockWave, 2f);
        Destroy(newParticles.gameObject, newParticles.main.duration); // Nettoie après la durée de l'effet
    }
    

    IEnumerator ExplosionRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float normalizedTime = elapsedTime / duration;
            
            
            S_GroundPound_Module groundPoundModule = FindObjectOfType<S_GroundPound_Module>();
            float range = groundPoundModule.DynamicSphereRange;
            //Debug.Log("Portée dynamique du Ground Pound : " + range);
            // Applique la courbe de taille avec un multiplicateur
            float scale = sizeCurve.Evaluate(normalizedTime) * range;
            sphereRenderer.transform.localScale = Vector3.one * scale;

            // Applique la transparence
            if (sphereMaterial != null)
            {
                Color color = sphereMaterial.color;
                color.a = alphaCurve.Evaluate(normalizedTime);
                sphereMaterial.color = color;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(currentExplosion);
    }
    
    
    public void Play(Vector3 origin)
    {
        currentDistance = 0f;
        isPlaying = true;
        ShockWavePoint.transform.position = origin;

        // Optionnel : si le shader a un paramètre "Enabled"
        shockwaveMaterial.SetFloat("_Shockwave_Enabled", 1f);
        shockwaveMaterial.SetFloat("_Shockwave_Max_Distance", maxDistance);
    }
}