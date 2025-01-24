using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate_Ring : MonoBehaviour
{
    // Vitesse de rotation autour des axes (en degrés par seconde)
    public Vector3 rotationSpeed = new Vector3(0, 100, 0);

    void Update()
    {
        // Applique une rotation continue basée sur le temps écoulé
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
