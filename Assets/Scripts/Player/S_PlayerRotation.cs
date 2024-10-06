using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerRotation : MonoBehaviour
{

    public float moveSpeed = 10f;  // Vitesse de d�placement
    public bool canMove = true;    // Bool�en pour activer ou d�sactiver le mouvement

    private Rigidbody rb;          // R�f�rence au Rigidbody

    void Start()
    {
        // Obtenir le Rigidbody attach� � l'objet
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        RotateTowardsMouse();
        if (canMove)
        {
            MovePlayer();
        }
    }

    // Faire tourner l'objet vers la position de la souris
    void RotateTowardsMouse()
    {
        // Obtenir la position de la souris en pixels � l'�cran
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convertir la position de la souris en coordonn�es du monde
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition); // Utiliser Camera.main pour obtenir la cam�ra principale
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Le plan de l'axe Y (horizontal)

        float rayLength;

        // Calculer o� le rayon coupe le plan du sol
        if (groundPlane.Raycast(ray, out rayLength))
        {
            // Obtenir le point d'impact dans le monde
            Vector3 pointToLook = ray.GetPoint(rayLength);

            // Utiliser LookAt pour que l'objet tourne vers le point de la souris, en verrouillant l'axe Y
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    void MovePlayer()
    {
        // R�cup�rer les entr�es brutes de l'utilisateur pour les axes X (gauche/droite) et Z (avant/arri�re)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Cr�er une direction de mouvement bas�e sur les axes X et Z (sans tenir compte de la rotation du joueur)
        Vector3 moveDirection = new Vector3(h, 0f, v);

        // Normaliser la direction pour �viter que le mouvement diagonal soit plus rapide
        Vector3 normMove = moveDirection.normalized;

        // Mettre � jour la v�locit� du Rigidbody pour d�placer le joueur uniquement sur l'axe XZ
        rb.velocity = new Vector3(normMove.x * moveSpeed, rb.velocity.y, normMove.z * moveSpeed);
    }
}
