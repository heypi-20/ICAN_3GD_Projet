using System;
using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    public GameObject explosionPrefab;  // Le prefab de l'explosion
    private GameObject currentExplosion; // Stocke l'objet instancié

    public AnimationCurve sizeCurve;
    public AnimationCurve alphaCurve;
    public float duration = 1f;
    public MeshRenderer sphereRenderer;  // Renseigner depuis l'inspecteur

    private Material sphereMaterial;
    public float maxScale = 3f; 


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            TriggerExplosion(gameObject.transform.position);
        }
    }

    public void TriggerExplosion(Vector3 impactPosition)
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

            // Applique la courbe de taille avec un multiplicateur
            float scale = sizeCurve.Evaluate(normalizedTime) * maxScale;
            currentExplosion.transform.localScale = Vector3.one * scale;

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