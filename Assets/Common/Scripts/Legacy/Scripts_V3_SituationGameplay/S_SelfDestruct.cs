using UnityEngine;
using System;

public class S_SelfDestruct : MonoBehaviour
{
    public int destroyThreshold = 5; // Seuil de destruction, modifiable dans l'Inspector
    private int resetCount = 0; // Compteur d'�v�nements OnZoneReset

    private void Start()
    {
        // Recherche de l'objet ayant le script S_ZoneResetSysteme dans la sc�ne
        S_ZoneResetSysteme zoneResetSysteme = FindObjectOfType<S_ZoneResetSysteme>();

        if (zoneResetSysteme != null)
        {
            // Abonnement � l'�v�nement OnZoneReset
            S_ZoneResetSysteme.OnZoneReset += IncrementResetCount;
        }
        else
        {
            Debug.LogWarning("Aucun objet avec le script S_ZoneResetSysteme trouv� dans la sc�ne");
        }
    }

    private void OnDestroy()
    {
        // D�sabonnement de l'�v�nement pour �viter les fuites de m�moire
        S_ZoneResetSysteme.OnZoneReset -= IncrementResetCount;
    }

    private void IncrementResetCount()
    {
        resetCount++; // Incr�mente le compteur chaque fois que l'�v�nement est d�clench�

        if (resetCount >= destroyThreshold)
        {
            Destroy(gameObject); // D�truit l'objet une fois le seuil atteint
            Debug.Log("Seuil de destruction atteint, l'objet est d�truit");
        }
    }
}
