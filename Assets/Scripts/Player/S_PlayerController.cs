using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class S_PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public bool canMove = true;    // Bool�en pour activer ou d�sactiver le mouvement

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
            // R�cup�rer les entr�es brutes de l'utilisateur pour les axes X (gauche/droite) et Z (avant/arri�re)
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");

            // Cr�er une direction de mouvement bas�e sur les axes X et Z (sans tenir compte de la rotation du joueur)
            Vector3 moveDirection = new Vector3(h, 0f, v);

            // Normaliser la direction pour �viter que le mouvement diagonal soit plus rapide
            Vector3 normMove = moveDirection.normalized;

            // Mettre � jour la v�locit� du Rigidbody pour d�placer le joueur uniquement sur l'axe XZ
            rb.velocity = new Vector3(normMove.x * moveSpeed, rb.velocity.y, normMove.z * moveSpeed);
        }
    }
}
