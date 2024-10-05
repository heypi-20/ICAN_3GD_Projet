using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]  // Permet à Unity de rendre cette classe visible dans l'Inspector
public class RespawnableObject
{
    public GameObject objectToRespawn;  // Objet à réapparaître ou à réinitialiser
    public Transform respawnLocation;   // Position de réapparition
    public bool shouldRespawn = false;  // Indique si l'objet doit être recréé au lieu d'être déplacé
}


public class S_ZoneResetSysteme : MonoBehaviour
{
    [Header("Réinitialisation des objets")]
    public List<RespawnableObject> respawnableObjects;  // Liste des objets réapparaissables

    public static event Action OnZoneReset;// Définir un événement pour que d'autres objets puissent s'y abonner   
    public TextMeshProUGUI resetCountText;// Composant TextMeshPro UI pour afficher le nombre de réinitialisations
    [SerializeField] private KeyCode resetKey = KeyCode.R;// Touche configurable pour déclencher la réinitialisation de la zone

    private int resetCount = 0; // Compteur du nombre de réinitialisations déclenchées

    void Update()
    {
        // Lorsque la touche définie est pressée, déclencher l'événement de réinitialisation
        if (Input.GetKeyDown(resetKey))
        {
            TriggerZoneReset();
        }
    }

    // Méthode pour déclencher l'événement de réinitialisation
    private void TriggerZoneReset()
    {
        Debug.Log("Réinitialisation de la zone déclenchée!");

        // Incrémenter le compteur de réinitialisations
        resetCount++;

        // Mettre à jour l'affichage du texte dans l'UI
        UpdateResetCountText();

        // Déclencher l'événement s'il y a des abonnés
        OnZoneReset?.Invoke();

        RespawnObjects();

    }

    private void RespawnObjects()
    {
        foreach (RespawnableObject respawnableObject in respawnableObjects)
        {
            if (respawnableObject.objectToRespawn != null && respawnableObject.respawnLocation != null)
            {
                if (respawnableObject.shouldRespawn)
                {
                    // Si l'objet doit être recréé (reset total)
                   
                    respawnableObject.objectToRespawn = Instantiate(
                        respawnableObject.objectToRespawn,
                        respawnableObject.respawnLocation.position,
                        respawnableObject.respawnLocation.rotation
                    );  // Recréer un nouvel objet
                }
                else
                {
                    // Si l'objet doit être déplacé à sa position initiale
                    respawnableObject.objectToRespawn.transform.position = respawnableObject.respawnLocation.position;
                    respawnableObject.objectToRespawn.transform.rotation = respawnableObject.respawnLocation.rotation;
                }
            }
            else
            {
                Debug.LogWarning("L'objet ou la position de réapparition n'est pas définie.");
            }
        }
    }


    // Méthode pour mettre à jour le texte TextMeshPro
    private void UpdateResetCountText()
    {
        if (resetCountText != null)
        {
            resetCountText.text = $"Nombre de réinitialisations : {resetCount}";
        }
        else
        {
            Debug.LogWarning("Le composant TextMeshProUGUI n'est pas assigné.");
        }
    }
}
