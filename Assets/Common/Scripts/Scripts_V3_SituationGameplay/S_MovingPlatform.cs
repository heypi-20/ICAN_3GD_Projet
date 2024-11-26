using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_MovingPlatform : MonoBehaviour
{
    public Vector3 pointA; // Point de d�part
    public Vector3 pointB; // Point d'arriv�e
    public float vitesse = 2f; // Vitesse du d�placement
    private float t = 0f; // Param�tre de progression (de 0 � 1)

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
