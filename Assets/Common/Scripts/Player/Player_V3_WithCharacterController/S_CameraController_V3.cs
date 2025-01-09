using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_CameraController_V3 : MonoBehaviour
{
    [Header("Paramètres du mouvement du joueur")]
    public Transform playerBody;  // Transform du joueur
    public Transform cameraTransform;  // Transform de la caméra

    private void Start()
    {
        // Verrouille le curseur de la souris au centre de l'écran
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Synchronise instantanément l'orientation du joueur avec la direction de la caméra
        AlignerJoueurAvecCamera();
    }

    /// <summary>
    /// Ajuste immédiatement la rotation du joueur pour qu'il fasse toujours face à la direction de la caméra
    /// </summary>
    void AlignerJoueurAvecCamera()
    {
        // Récupère la direction avant de la caméra, en ignorant la rotation verticale
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;  // Ignore l'axe Y pour garder une rotation horizontale
        cameraForward.Normalize();

        // Applique directement la rotation cible au joueur
        playerBody.rotation = Quaternion.LookRotation(cameraForward);
    }
}
