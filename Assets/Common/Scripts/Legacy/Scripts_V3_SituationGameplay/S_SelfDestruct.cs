using UnityEngine;
using System;

public class S_SelfDestruct : MonoBehaviour
{
    public int destroyThreshold = 5; // Seuil de destruction, modifiable dans l'Inspector
    private int resetCount = 0; // Compteur d'événements OnZoneReset

    private void Start()
    {
        // Recherche de l'objet ayant le script S_ZoneResetSysteme dans la scène
        S_ZoneResetSysteme zoneResetSysteme = FindObjectOfType<S_ZoneResetSysteme>();

        if (zoneResetSysteme != null)
        {
            // Abonnement à l'événement OnZoneReset
            S_ZoneResetSysteme.OnZoneReset += IncrementResetCount;
        }
        else
        {
            Debug.LogWarning("Aucun objet avec le script S_ZoneResetSysteme trouvé dans la scène");
        }
    }

    private void OnDestroy()
    {
        // Désabonnement de l'événement pour éviter les fuites de mémoire
        S_ZoneResetSysteme.OnZoneReset -= IncrementResetCount;
    }

    private void IncrementResetCount()
    {
        resetCount++; // Incrémente le compteur chaque fois que l'événement est déclenché

        if (resetCount >= destroyThreshold)
        {
            Destroy(gameObject); // Détruit l'objet une fois le seuil atteint
            Debug.Log("Seuil de destruction atteint, l'objet est détruit");
        }
    }
}
