using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerRotation : MonoBehaviour
{

    public float moveSpeed = 10f;  // Vitesse de déplacement
    public bool canMove = true;    // Booléen pour activer ou désactiver le mouvement

    private Rigidbody rb;          // Référence au Rigidbody

    void Start()
    {
        // Obtenir le Rigidbody attaché à l'objet
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

    void MovePlayer()
    {
        // Récupérer les entrées brutes de l'utilisateur pour les axes X (gauche/droite) et Z (avant/arrière)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Créer une direction de mouvement basée sur les axes X et Z (sans tenir compte de la rotation du joueur)
        Vector3 moveDirection = new Vector3(h, 0f, v);

        // Normaliser la direction pour éviter que le mouvement diagonal soit plus rapide
        Vector3 normMove = moveDirection.normalized;

        // Mettre à jour la vélocité du Rigidbody pour déplacer le joueur uniquement sur l'axe XZ
        rb.velocity = new Vector3(normMove.x * moveSpeed, rb.velocity.y, normMove.z * moveSpeed);
    }
}
