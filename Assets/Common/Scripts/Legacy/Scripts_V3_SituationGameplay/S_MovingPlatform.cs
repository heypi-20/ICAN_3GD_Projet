using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_MovingPlatform : MonoBehaviour
{
    public Vector3 pointA; // Point de départ
    public Vector3 pointB; // Point d'arrivée
    public float vitesse = 2f; // Vitesse du déplacement
    private float t = 0f; // Paramètre de progression (de 0 à 1)

    private bool StopForASec = true;
    private bool PlayerIsOn = false;
    public Vector3 Origin;

    private void Start()
    {
        Origin = gameObject.transform.position;
    }
    private void Update()
    {
        if(PlayerIsOn!)
        {
            transform.position = Origin;
        }

        if(StopForASec && PlayerIsOn)
        {
            // Lerp entre le point A et B en fonction de t
            t += Time.deltaTime * vitesse; // Incrementation de t selon le temps écoulé
            if (t > 1f) t = 1f; // Limite t à 1 pour ne pas dépasser la destination

            // Déplacer la plateforme
            transform.position = Vector3.Lerp(pointA, pointB, t);


            // Revenir au point A une fois arrivé à B
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
    private void FixedUpdate()
    {
        PlayerIsOn = false;
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            PlayerIsOn = true;
        }
    }
    IEnumerator MovePlat()
    {
        yield return new WaitForSeconds(1);
        StopForASec = true;
    }
}
