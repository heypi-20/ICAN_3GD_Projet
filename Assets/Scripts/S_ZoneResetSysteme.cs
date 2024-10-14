using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]  // Permet à Unity de rendre cette classe visible dans l'Inspector
public class RespawnableObject
{
    [Header("Prefab Settings")]
    [Tooltip("Prefab utilisé pour recréer l'objet.")]
    public GameObject prefabToRespawn;  // Préfabriqué utilisé pour recréer l'objet

    [Tooltip("Référence à l'instance actuelle de l'objet.")]
    public GameObject currentInstance;  // Référence à l'instance actuelle générée
    
    [Tooltip("Position où l'objet réapparaîtra.")]
    public Transform respawnLocation;   // Position de réapparition
    
    [Tooltip("Indique si l'objet doit être recréé au lieu d'être déplacé.")]
    public bool shouldRespawn = false;  // Indique si l'objet doit être recréé au lieu d'être déplacé
    
    [Tooltip("Indique si l'instance précédente doit être détruite si elle existe.")]
    public bool destroyPrevious = false;  // Détermine si l'objet précédent doit être détruit s'il existe
    
    [Tooltip("Indique si l'objet tenu par le joueur doit être détruit.")]
    public bool destroyPlayerItem; // Indique si l'objet tenu par le joueur doit être détruit
    
    [Tooltip("Nombre de tours avant la réapparition ou la réinitialisation de l'objet.")]
    public int respawnAfterRounds = 1;  // Nombre de tours avant de réapparaître ou réinitialiser l'objet
    
    [HideInInspector]
    public int currentRounds = 0;  // Compteur du nombre de tours actuels avant réapparition
    [HideInInspector]
    public bool isHandledByPlayer = false; // Marque si l'objet a été pris par le joueur
}

public class S_ZoneResetSysteme : MonoBehaviour
{
    public static event Action OnZoneReset; // Définir un événement pour que d'autres objets puissent s'y abonner 
    [Header("Respawnable Objects List and Reset Settings")]
    [Tooltip("Liste des objets pouvant réapparaître dans la zone.")]
    public List<RespawnableObject> respawnableObjects;  // Liste des objets réapparaissables

    [Header("UI Settings")]
    [Tooltip("Composant TextMeshPro pour afficher le nombre de réinitialisations.")]
    public TextMeshProUGUI resetCountText; // Composant TextMeshPro UI pour afficher le nombre de réinitialisations

    [Header("Input Settings")]
    [Tooltip("Touche pour déclencher la réinitialisation de la zone.")]
    [SerializeField] private KeyCode resetKey = KeyCode.R; // Touche configurable pour déclencher la réinitialisation de la zone

    [Tooltip("Compteur du nombre de réinitialisations déclenchées.")]
    private int resetCount = 0; // Compteur du nombre de réinitialisations déclenchées

    void Update()  // Vérifier l'entrée utilisateur à chaque frame pour déclencher la réinitialisation
    {
        // Lorsque la touche définie est pressée, déclencher l'événement de réinitialisation
        if (Input.GetKeyDown(resetKey))
        {
            TriggerZoneReset();
        }
    }

    // Méthode pour déclencher l'événement de réinitialisation
    private void TriggerZoneReset()  // Méthode pour déclencher l'événement de réinitialisation de la zone
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

    private void RespawnObjects()  // Réapparaître les objets selon leurs paramètres après chaque réinitialisation
    {
        foreach (RespawnableObject respawnableObject in respawnableObjects)
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

    private void HandleObjectRespawn(RespawnableObject respawnableObject)  // Gérer la logique de réapparition ou de déplacement de chaque objet
    {
        // Si l'option de détruire le précédent objet est activée
        if (respawnableObject.destroyPrevious && respawnableObject.currentInstance != null)
        {
            if (respawnableObject.currentInstance.GetComponent<CaughtByPlayer>() == null)
            {
                if (respawnableObject.currentInstance.GetComponent<S_CultivableCube>() == null)
                {
                    // Si l'objet n'a pas été pris par le joueur et ne contient pas le script du cube cultivable
                    //, le détruire.
                    Destroy(respawnableObject.currentInstance);
                }
                else if (!respawnableObject.currentInstance.GetComponent<S_CultivableCube>().isGrowing)
                {
                    Destroy(respawnableObject.currentInstance);
                }
               
            }
            else
            {
                // Si l'objet a été pris par le joueur, marquer comme pris
                respawnableObject.isHandledByPlayer = true;
            }
        }

        // Si l'option de détruire l'objet pris par le joueur est activée
        if (respawnableObject.destroyPlayerItem && respawnableObject.isHandledByPlayer)
        {
            Destroy(respawnableObject.currentInstance);
            respawnableObject.isHandledByPlayer = false; // Réinitialiser le marqueur
        }

        // Recréer ou déplacer l'objet
        if (respawnableObject.shouldRespawn)
        {
            // Recréer un nouvel objet à partir du prefab
            respawnableObject.currentInstance = Instantiate(
                respawnableObject.prefabToRespawn,
                respawnableObject.respawnLocation.position,
                respawnableObject.respawnLocation.rotation
            );
        }
        
        else
        {
            if (!respawnableObject.shouldRespawn)
            {
                respawnableObject.currentInstance= respawnableObject.prefabToRespawn;
            }
            
            // Déplacer l'objet à sa position initiale
            if (respawnableObject.currentInstance != null)
            {
                if (respawnableObject.currentInstance.GetComponent<S_CultivableCube>()==null)
                {
                    respawnableObject.currentInstance.transform.position = respawnableObject.respawnLocation.position;
                    respawnableObject.currentInstance.transform.rotation = respawnableObject.respawnLocation.rotation;
                }
                else if (!respawnableObject.currentInstance.GetComponent<S_CultivableCube>().isGrowing)
                {
                    respawnableObject.currentInstance.transform.position = respawnableObject.respawnLocation.position;
                    respawnableObject.currentInstance.transform.rotation = respawnableObject.respawnLocation.rotation;
                }
                
            }
        }
    }

    // Méthode pour mettre à jour le texte TextMeshPro
    private void UpdateResetCountText()  // Mettre à jour le texte UI qui affiche le nombre de réinitialisations
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