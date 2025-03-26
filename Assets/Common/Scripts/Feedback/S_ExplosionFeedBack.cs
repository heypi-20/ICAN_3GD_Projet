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
    
    private IEnumerator WaitForObserver()
    {
        float timeout = 3f;
        float elapsedTime = 0f;

        while (S_PlayerStateObserver.Instance == null)
        {
            if (elapsedTime >= timeout)
            {
                Debug.LogError("S_PlayerStateObserver.Instance not found after waiting " + timeout + " seconds.");
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
            Debug.Log("Ca devrait exploser ici");
            TriggerExplosion(Explosion_GroundPound_SpawnPoint.transform.position);
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
    

    IEnumerator ExplosionRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float normalizedTime = elapsedTime / duration;
            
            
            S_GroundPound_Module groundPoundModule = FindObjectOfType<S_GroundPound_Module>();
            float range = groundPoundModule.DynamicSphereRange;
            Debug.Log("Portée dynamique du Ground Pound : " + range);
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
}