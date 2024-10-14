using UnityEngine;

public class S_RemoveComponent : MonoBehaviour
{
    
    [Header("Speed Settings")]
    [Tooltip("La vitesse minimale en dessous de laquelle l'objet est considéré comme immobile")]
    public float minSpeed = 0f;

    private Rigidbody rb;
    private bool hasCheckedForThrownScript = false; // Indique si le script ThrownByThePlayer a déjà été vérifié

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing on this object.");
            return;
        }
    }

    void Update()
    {
        // Vérifier si la vitesse est inférieure au seuil pour détecter l'immobilité
        if (rb.velocity.sqrMagnitude <= minSpeed * minSpeed)
        {
            // Si l'objet est immobile et que le script n'a pas encore été vérifié
            if (!hasCheckedForThrownScript)
            {
                ThrownByThePlayer thrownScript = GetComponent<ThrownByThePlayer>();

                if (thrownScript != null)
                {
                    // Si le script ThrownByThePlayer existe, le détruire
                    Destroy(thrownScript);
                }

                // Marquer que le script a été vérifié pour éviter des répétitions inutiles
                hasCheckedForThrownScript = true;
            }
        }
        else
        {
            // Si l'objet se remet en mouvement, réinitialiser l'état pour vérifier de nouveau à l'arrêt
            hasCheckedForThrownScript = false;
        }
    }
}
