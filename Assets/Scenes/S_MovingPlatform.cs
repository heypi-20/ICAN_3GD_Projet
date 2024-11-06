using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MovingPlatform : MonoBehaviour
{
    public Vector3 pointA; // Point de départ
    public Vector3 pointB; // Point d'arrivée
    public float vitesse = 2f; // Vitesse du déplacement
    private float t = 0f; // Paramètre de progression (de 0 à 1)

    private void Update()
    {
        // Lerp entre le point A et B en fonction de t
        t += Time.deltaTime * vitesse; // Incrementation de t selon le temps écoulé
        if (t > 1f) t = 1f; // Limite t à 1 pour ne pas dépasser la destination

        // Déplacer la plateforme
        transform.position = Vector3.Lerp(pointA, pointB, t);

        // Revenir au point A une fois arrivé à B
        if (t >= 1f)
        {
            // Inverser les points pour faire revenir la plateforme au point A
            Vector3 temp = pointA;
            pointA = pointB;
            pointB = temp;

            // Réinitialiser t pour recommencer le mouvement
            t = 0f;
        }
    }
}
