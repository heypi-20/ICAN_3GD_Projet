using UnityEngine;

public class S_RemoveComponent : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("La vitesse minimale en dessous de laquelle l'objet est considéré comme immobile")]
    public float minSpeed = 0f;

    [Header("Delay Settings")]
    [Tooltip("Le délai en secondes avant de détruire le script ThrownByThePlayer une fois l'objet immobile")]
    public float delayBeforeDestroy = 0.5f; // Délai de 0.5 secondes par défaut

    private Rigidbody rb;
    private bool hasCheckedForThrownScript = false; // Indique si le script ThrownByThePlayer a déjà été vérifié
    private float timer = 0f; // Timer pour gérer le délai

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
            // Commencer à incrémenter le timer si l'objet est immobile
            timer += Time.deltaTime;

            // Si le timer atteint le délai et que le script n'a pas encore été vérifié
            if (timer >= delayBeforeDestroy && !hasCheckedForThrownScript)
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
            // Réinitialiser le timer et l'état si l'objet se remet en mouvement
            timer = 0f;
            hasCheckedForThrownScript = false;
        }
    }
}
