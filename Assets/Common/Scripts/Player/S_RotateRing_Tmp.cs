using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_RotateRing_Tmp : MonoBehaviour
{
    // Vitesse de rotation autour des axes (en degr�s par seconde)
    public Vector3 rotationSpeed = new Vector3(0, 0, 100);
    public Vector3 BaseRotationSpeed = new Vector3(0, 0, 100);
    public S_InputManager _inputManager;
    public float Acceleration = 3f;

    void Update()
    {
        // Applique une rotation continue bas�e sur le temps �coul�
        transform.Rotate(rotationSpeed * Time.deltaTime);

        if (_inputManager.ShootInput)
        {
            // Augmente la vitesse de rotation pendant que le joueur tire
            rotationSpeed = BaseRotationSpeed * Acceleration;
        }
        else
        {
            // Restaure la vitesse de rotation de base lorsque le joueur arr�te de tirer
            rotationSpeed = BaseRotationSpeed;
        }
    }
}
