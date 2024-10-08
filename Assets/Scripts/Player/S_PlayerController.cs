using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public bool canMove = true;    // Booléen pour activer ou désactiver le mouvement

    private float h;
    private float v;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void MovePlayer()
    {
        if (canMove)
        {
            // Récupérer les entrées brutes de l'utilisateur pour les axes X (gauche/droite) et Z (avant/arrière)
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

            // Créer une direction de mouvement basée sur les axes X et Z (sans tenir compte de la rotation du joueur)
            Vector3 moveDirection = new Vector3(h, 0f, v);

            // Normaliser la direction pour éviter que le mouvement diagonal soit plus rapide
            Vector3 normMove = moveDirection.normalized;

            // Mettre à jour la vélocité du Rigidbody pour déplacer le joueur uniquement sur l'axe XZ
            rb.velocity = new Vector3(normMove.x * moveSpeed, rb.velocity.y, normMove.z * moveSpeed);
        }
    }
}
