using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantPropa : MonoBehaviour
{
    public float _Chrono = 5f;
    public float Chornotarget = 3f;
    private int Growth;
    public bool CanGrow = true;
    public float growthRate = 0.5f;
    // Update is called once per frame
    void Update()
    {
        _Chrono -= Time.deltaTime;

        if (_Chrono < 0 && CanGrow)
        {
            // Calcul de la taille cible.
            Vector3 targetScale = new Vector3(
                gameObject.transform.localScale.x + growthRate,
                0.4f,
                gameObject.transform.localScale.z + growthRate
            );
            StartCoroutine(Growing(targetScale, 0.5f)); // 0.5f est la dur�e de l'animation.

            // R�initialisation du chronom�tre et incr�mentation logique.
            _Chrono = Chornotarget;
            Growth += 1;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.CompareTag("Seed"))
        {
            CanGrow = false;
            Destroy(gameObject);
            for(int i = Growth; i > 0; i--)
            {
                Instantiate(collision.gameObject);
                Growth -= 1;
            }
        }
    }

    IEnumerator Growing(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = gameObject.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            gameObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }

        gameObject.transform.localScale = targetScale;
    }
}
