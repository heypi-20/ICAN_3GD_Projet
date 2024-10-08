using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerRotation : MonoBehaviour
{
    private Rigidbody rb;          // Référence au Rigidbody
    private S_PlayerController pc;

    void Start()
    {
        // Obtenir le Rigidbody attaché à l'objet
        rb = GetComponent<Rigidbody>();
        
        pc = GetComponent<S_PlayerController>();
    }
    void Update()
    {
        RotateTowardsMouse();

        pc.MovePlayer();
    }

    // Faire tourner l'objet vers la position de la souris
    void RotateTowardsMouse()
    {
        // Obtenir la position de la souris en pixels à l'écran
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convertir la position de la souris en coordonnées du monde
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition); // Utiliser Camera.main pour obtenir la caméra principale
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Le plan de l'axe Y (horizontal)

        float rayLength;

        // Calculer où le rayon coupe le plan du sol
        if (groundPlane.Raycast(ray, out rayLength))
        {
            // Obtenir le point d'impact dans le monde
            Vector3 pointToLook = ray.GetPoint(rayLength);

            // Utiliser LookAt pour que l'objet tourne vers le point de la souris, en verrouillant l'axe Y
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }
}
