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
        // Ajouter automatiquement et r�f�rencer S_PlayerResetModule et S_PlayerSpawnObjects
        playerResetModule = GetComponent<S_PlayerResetModule>();
        playerSpawnObjects = GetComponent<S_PlayerResetSpawnModule>();
    }

    private void OnEnable()
    {
        if (playerResetModule != null)
        {
            // �couter l'�v�nement PlayerResetEvent du module de r�initialisation du joueur
            playerResetModule.PlayerResetEvent += HandlePlayerReset;
        }
    }

    private void OnDisable()
    {
        if (playerResetModule != null)
        {
            // Supprimer l'�coute de l'�v�nement PlayerResetEvent
            playerResetModule.PlayerResetEvent -= HandlePlayerReset;
        }
    }

    private void HandlePlayerReset()
    {
        Debug.Log("�v�nement PlayerReset d�tect�. Lancer l'apparition des objets...");

        if (playerSpawnObjects != null)
        {
            // Appeler la m�thode SpawnObjects du module de spawn
            playerSpawnObjects.PlayerSpawnObjects();
        }
        else
        {
            Debug.LogWarning("Le module de spawn n'est pas correctement assign�.");
        }
    }
}
