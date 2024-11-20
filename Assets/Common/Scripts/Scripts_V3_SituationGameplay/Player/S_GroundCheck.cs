using UnityEngine;

public class S_GroundCheck : MonoBehaviour
{
    [Tooltip("Le rayon utilisé pour la détection de contact avec le sol")]
    public float sphereRadius = 0.5f;
    [Tooltip("La distance de projection utilisée pour détecter le sol sous le joueur")]
    public float sphereCastDistance = 1f;
    [Tooltip("Masque de couche pour ignorer les collisions avec l'objet")]
    public LayerMask IgnoreMask;

    // Indique si le joueur est au sol ou non
    public bool IsGrounded { get; private set; }

    void Update()
    {
        GroundCheckMethod();  // Vérification du sol à chaque frame
    }

    private void GroundCheckMethod()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;

        // Utilisation d'un SphereCast pour détecter le sol tout en ignorant le joueur et ses enfants
        IsGrounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out hit, sphereCastDistance, ~IgnoreMask);
    }

    // Méthode Gizmos pour visualiser le SphereCast dans l'éditeur et ajuster les paramètres
    private void OnDrawGizmos()
    {
        // Changer la couleur en fonction de l'état du joueur (sur le sol ou non)
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        // Dessiner la sphère qui montre la zone de détection
        Gizmos.DrawWireSphere(transform.position + Vector3.down * sphereCastDistance, sphereRadius);
    }
}
