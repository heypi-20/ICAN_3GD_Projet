using UnityEngine;
using System;

[RequireComponent(typeof(S_PlayerTeleportModule))]
[RequireComponent(typeof(S_PlayerResetModule))]
public class S_TeleportPlayerOnPlayerReset : MonoBehaviour
{
    public bool useSceneResetManager = true; // Si activé, chercher automatiquement le ResetManager dans la scène

    public S_PlayerResetModule playerResetModule;
    private S_PlayerTeleportModule playerTeleportModule;

    private void Awake()
    {
        if (useSceneResetManager)
        {
            // Chercher automatiquement le ResetManager dans la scène si nécessaire
            playerResetModule = FindObjectOfType<S_PlayerResetModule>();
        }
        else
        {
            // Utilisateur doit fournir une référence au composant S_PlayerResetModule
            playerResetModule = GetComponent<S_PlayerResetModule>();
        }
        // Référencer S_PlayerTeleportModule
        playerTeleportModule = GetComponent<S_PlayerTeleportModule>();
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
        Debug.Log("Événement PlayerReset détecté. Incrémenter le compteur d'appels et potentiellement téléporter le joueur...");

        if (playerTeleportModule != null)
        {
            // Appeler la méthode IncrementCallCountAndTeleport du module de téléportation
            playerTeleportModule.IncrementCallCountAndTeleport();
        }
        else
        {
            Debug.LogWarning("Le module de téléportation n'est pas correctement assigné.");
        }
    }
}
