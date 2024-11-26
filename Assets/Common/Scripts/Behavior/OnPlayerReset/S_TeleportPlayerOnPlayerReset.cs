using UnityEngine;
using System;

[RequireComponent(typeof(S_PlayerTeleportModule))]
[RequireComponent(typeof(S_PlayerResetModule))]
public class S_TeleportPlayerOnPlayerReset : MonoBehaviour
{
    public bool useSceneResetManager = true; // Si activ�, chercher automatiquement le ResetManager dans la sc�ne

    public S_PlayerResetModule playerResetModule;
    private S_PlayerTeleportModule playerTeleportModule;

    private void Awake()
    {
        if (useSceneResetManager)
        {
            // Chercher automatiquement le ResetManager dans la sc�ne si n�cessaire
            playerResetModule = FindObjectOfType<S_PlayerResetModule>();
        }
        else
        {
            // Utilisateur doit fournir une r�f�rence au composant S_PlayerResetModule
            playerResetModule = GetComponent<S_PlayerResetModule>();
        }
        // R�f�rencer S_PlayerTeleportModule
        playerTeleportModule = GetComponent<S_PlayerTeleportModule>();
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
        Debug.Log("�v�nement PlayerReset d�tect�. Incr�menter le compteur d'appels et potentiellement t�l�porter le joueur...");

        if (playerTeleportModule != null)
        {
            // Appeler la m�thode IncrementCallCountAndTeleport du module de t�l�portation
            playerTeleportModule.IncrementCallCountAndTeleport();
        }
        else
        {
            Debug.LogWarning("Le module de t�l�portation n'est pas correctement assign�.");
        }
    }
}
