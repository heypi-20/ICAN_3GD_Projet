using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]  // Permet � Unity de rendre cette classe visible dans l'Inspector
public class RespawnableObject
{
    [Header("Prefab Settings")]
    [Tooltip("Prefab utilis� pour recr�er l'objet.")]
    public GameObject prefabToRespawn;  // Pr�fabriqu� utilis� pour recr�er l'objet

    [Tooltip("R�f�rence � l'instance actuelle de l'objet.")]
    public GameObject currentInstance;  // R�f�rence � l'instance actuelle g�n�r�e
    
    [Tooltip("Position o� l'objet r�appara�tra.")]
    public Transform respawnLocation;   // Position de r�apparition
    
    [Tooltip("Indique si l'objet doit �tre recr�� au lieu d'�tre d�plac�.")]
    public bool shouldRespawn = false;  // Indique si l'objet doit �tre recr�� au lieu d'�tre d�plac�
    
    [Tooltip("Indique si l'instance pr�c�dente doit �tre d�truite si elle existe.")]
    public bool destroyPrevious = false;  // D�termine si l'objet pr�c�dent doit �tre d�truit s'il existe
    
    [Tooltip("Indique si l'objet tenu par le joueur doit �tre d�truit.")]
    public bool destroyPlayerItem; // Indique si l'objet tenu par le joueur doit �tre d�truit
    
    [Tooltip("Nombre de tours avant la r�apparition ou la r�initialisation de l'objet.")]
    public int respawnAfterRounds = 1;  // Nombre de tours avant de r�appara�tre ou r�initialiser l'objet
    
    [HideInInspector]
    public int currentRounds = 0;  // Compteur du nombre de tours actuels avant r�apparition
    [HideInInspector]
    public bool isHandledByPlayer = false; // Marque si l'objet a �t� pris par le joueur
}

public class S_ZoneResetSysteme : MonoBehaviour
{
    public static event Action OnZoneReset; // D�finir un �v�nement pour que d'autres objets puissent s'y abonner 
    [Header("Respawnable Objects List and Reset Settings")]
    [Tooltip("Liste des objets pouvant r�appara�tre dans la zone.")]
    public List<RespawnableObject> respawnableObjects;  // Liste des objets r�apparaissables

    [Header("UI Settings")]
    [Tooltip("Composant TextMeshPro pour afficher le nombre de r�initialisations.")]
    public TextMeshProUGUI resetCountText; // Composant TextMeshPro UI pour afficher le nombre de r�initialisations

    [Header("Input Settings")]
    [Tooltip("Touche pour d�clencher la r�initialisation de la zone.")]
    [SerializeField] private KeyCode resetKey = KeyCode.R; // Touche configurable pour d�clencher la r�initialisation de la zone

    [Tooltip("Compteur du nombre de r�initialisations d�clench�es.")]
    private int resetCount = 0; // Compteur du nombre de r�initialisations d�clench�es

    void Update()  // V�rifier l'entr�e utilisateur � chaque frame pour d�clencher la r�initialisation
    {
        // Lorsque la touche d�finie est press�e, d�clencher l'�v�nement de r�initialisation
        if (Input.GetKeyDown(resetKey))
        {
            TriggerZoneReset();
        }
    }

    // M�thode pour d�clencher l'�v�nement de r�initialisation
    private void TriggerZoneReset()  // M�thode pour d�clencher l'�v�nement de r�initialisation de la zone
    {
        Debug.Log("Zone resets activated!");

        // Incr�menter le compteur de r�initialisations
        resetCount++;

        // Mettre � jour l'affichage du texte dans l'UI
        UpdateResetCountText();

        // D�clencher l'�v�nement s'il y a des abonn�s
        OnZoneReset?.Invoke();

        RespawnObjects();
    }

    private void RespawnObjects()  // R�appara�tre les objets selon leurs param�tres apr�s chaque r�initialisation
    {
        foreach (RespawnableObject respawnableObject in respawnableObjects)
        {
            respawnableObject.currentRounds++;

            // V�rifier si le nombre de tours requis est atteint
            if (respawnableObject.currentRounds < respawnableObject.respawnAfterRounds) continue;

            if (respawnableObject.prefabToRespawn == null || respawnableObject.respawnLocation == null)
            {
                Debug.LogWarning("Prefab or respawn position not defined.");
                continue;
            }

            HandleObjectRespawn(respawnableObject);
            respawnableObject.currentRounds = 0; // R�initialiser le compteur apr�s la r�apparition
        }
    }

    private void HandleObjectRespawn(RespawnableObject respawnableObject)  // G�rer la logique de r�apparition ou de d�placement de chaque objet
    {
        // Si l'option de d�truire le pr�c�dent objet est activ�e
        if (respawnableObject.destroyPrevious && respawnableObject.currentInstance != null)
        {
            if (respawnableObject.currentInstance.GetComponent<CaughtByPlayer>() == null)
            {
                if (respawnableObject.currentInstance.GetComponent<S_CultivableCube>() == null)
                {
                    // Si l'objet n'a pas �t� pris par le joueur et ne contient pas le script du cube cultivable
                    //, le d�truire.
                    Destroy(respawnableObject.currentInstance);
                }
                else if (!respawnableObject.currentInstance.GetComponent<S_CultivableCube>().isGrowing)
                {
                    Destroy(respawnableObject.currentInstance);
                }
               
            }
            else
            {
                // Si l'objet a �t� pris par le joueur, marquer comme pris
                respawnableObject.isHandledByPlayer = true;
            }
        }

        // Si l'option de d�truire l'objet pris par le joueur est activ�e
        if (respawnableObject.destroyPlayerItem && respawnableObject.isHandledByPlayer)
        {
            Destroy(respawnableObject.currentInstance);
            respawnableObject.isHandledByPlayer = false; // R�initialiser le marqueur
        }

        // Recr�er ou d�placer l'objet
        if (respawnableObject.shouldRespawn)
        {
            // Recr�er un nouvel objet � partir du prefab
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
            
            // D�placer l'objet � sa position initiale
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

    // M�thode pour mettre � jour le texte TextMeshPro
    private void UpdateResetCountText()  // Mettre � jour le texte UI qui affiche le nombre de r�initialisations
    {
        if (resetCountText != null)
        {
            resetCountText.text = $"Nombre de r�initialisations : {resetCount}";
        }
        else
        {
            Debug.LogWarning("The TextMeshProUGUI component isn't assigned.");
        }
    }
}