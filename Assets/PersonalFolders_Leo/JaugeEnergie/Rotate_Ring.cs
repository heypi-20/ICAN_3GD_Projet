using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate_Ring : MonoBehaviour
{
    // Vitesse de rotation autour des axes (en degrés par seconde)
    public Vector3 rotationSpeed = new Vector3(0, 0, 100);
    public Vector3 BaseRotationSpeed = new Vector3(0, 0, 100);
    public S_InputManager _inputManager;
    public float Acceleration = 3f;

    void Update()
    {
        // Applique une rotation continue basée sur le temps écoulé
        transform.Rotate(rotationSpeed * Time.deltaTime);

        if (_inputManager.ShootInput)
        {
            // Augmente la vitesse de rotation pendant que le joueur tire
            rotationSpeed = BaseRotationSpeed * Acceleration;
        }
        else
        {
            // Restaure la vitesse de rotation de base lorsque le joueur arrête de tirer
            rotationSpeed = BaseRotationSpeed;
        }
    }
}
