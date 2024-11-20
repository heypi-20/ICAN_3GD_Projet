using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]  // Permet à Unity de rendre cette classe visible dans l'Inspector

public class Old_RespawnableObject
{
    public GameObject prefabToRespawn;  // Préfabriqué utilisé pour recréer l'objet
    [HideInInspector]
    public GameObject currentInstance;  // Référence à l'instance actuelle générée
    public Transform respawnLocation;   // Position de réapparition
    public bool shouldRespawn = false;  // Indique si l'objet doit être recréé au lieu d'être déplacé
    public bool destroyPrevious = false;  // Détermine si l'objet précédent doit être détruit s'il existe
    [Tooltip("Number of round before object's respawn or resets.")]
    public int respawnAfterRounds = 1;  // Nombre de tours avant de réapparaître ou réinitialiser l'objet
    [HideInInspector]
    public int currentRounds = 0;  // Compteur du nombre de tours actuels avant réapparition
}


public class S_Old_ZoneResetSysteme : MonoBehaviour
{
    [Header("Reset of the objects")]
    public List<Old_RespawnableObject> respawnableObjects;  // Liste des objets réapparaissables
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
        Debug.Log("Zone resets activated!");

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
        foreach (Old_RespawnableObject respawnableObject in respawnableObjects)
        {
            respawnableObject.currentRounds++;

            // Vérifier si le nombre de tours requis est atteint
            if (respawnableObject.currentRounds < respawnableObject.respawnAfterRounds) continue;

            if (respawnableObject.prefabToRespawn == null || respawnableObject.respawnLocation == null)
            {
                Debug.LogWarning("Prefab or respawn position not defined.");
                continue;
            }

            HandleObjectRespawn(respawnableObject);
            respawnableObject.currentRounds = 0; // Réinitialiser le compteur après la réapparition
        }
    }

    private void HandleObjectRespawn(Old_RespawnableObject respawnableObject)
    {
        // Si l'option de détruire le précédent objet est activée
        if (respawnableObject.destroyPrevious && respawnableObject.currentInstance != null)
        {
            Destroy(respawnableObject.currentInstance);
        }

        if (respawnableObject.shouldRespawn)
        {
            // Recréer un nouvel objet à partir du prefab
            respawnableObject.currentInstance = Instantiate(
                respawnableObject.prefabToRespawn,
                respawnableObject.respawnLocation.position,
                respawnableObject.respawnLocation.rotation
            );
        }
        else if (!respawnableObject.shouldRespawn)
        {
            // Déplacer l'objet à sa position initiale
            respawnableObject.prefabToRespawn.transform.position = respawnableObject.respawnLocation.position;
            respawnableObject.prefabToRespawn.transform.rotation = respawnableObject.respawnLocation.rotation;
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
            Debug.LogWarning("The TextMeshProUGUI component isn't assigned.");
        }
    }
}
