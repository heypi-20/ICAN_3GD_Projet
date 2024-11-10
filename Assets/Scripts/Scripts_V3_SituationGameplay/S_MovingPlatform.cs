using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MovingPlatform : MonoBehaviour
{
    public Vector3 pointA; // Point de d�part
    public Vector3 pointB; // Point d'arriv�e
    public float vitesse = 2f; // Vitesse du d�placement
    private float t = 0f; // Param�tre de progression (de 0 � 1)

    private bool StopForASec = true;

    private void Update()
    {
        if(StopForASec)
        {
            // Lerp entre le point A et B en fonction de t
            t += Time.deltaTime * vitesse; // Incrementation de t selon le temps �coul�
            if (t > 1f) t = 1f; // Limite t � 1 pour ne pas d�passer la destination

            // D�placer la plateforme
            transform.position = Vector3.Lerp(pointA, pointB, t);


            // Revenir au point A une fois arriv� � B
            if (t >= 1f)
            {
                t = 0f;
                StopForASec = false;
                StartCoroutine(MovePlat());
                // Inverser les points pour faire revenir la plateforme au point A
                Vector3 temp = pointA;
                pointA = pointB;
                pointB = temp;
            }
        }        
    }

    IEnumerator MovePlat()
    {
        yield return new WaitForSeconds(1);
        StopForASec = true;
    }
}
