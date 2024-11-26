using UnityEngine;
using System;

[RequireComponent(typeof(S_PlayerResetModule))]
[RequireComponent(typeof(S_PlayerResetSpawnModule))]
public class S_PlayerSpawnOnPlayerReset : MonoBehaviour
{
    private S_PlayerResetModule playerResetModule;
    private S_PlayerResetSpawnModule playerSpawnObjects;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PlayerResetModule et S_PlayerSpawnObjects
        playerResetModule = GetComponent<S_PlayerResetModule>();
        playerSpawnObjects = GetComponent<S_PlayerResetSpawnModule>();
    }

    private void OnEnable()
    {
        if (playerResetModule != null)
        {
            // Écouter l'événement PlayerResetEvent du module de réinitialisation du joueur
            playerResetModule.PlayerResetEvent += HandlePlayerReset;
        }
    }

    private void OnDisable()
    {
        if (playerResetModule != null)
        {
            // Supprimer l'écoute de l'événement PlayerResetEvent
            playerResetModule.PlayerResetEvent -= HandlePlayerReset;
        }
    }

    private void HandlePlayerReset()
    {
        Debug.Log("Événement PlayerReset détecté. Lancer l'apparition des objets...");

        if (playerSpawnObjects != null)
        {
            // Appeler la méthode SpawnObjects du module de spawn
            playerSpawnObjects.PlayerSpawnObjects();
        }
        else
        {
            Debug.LogWarning("Le module de spawn n'est pas correctement assigné.");
        }
    }
}
