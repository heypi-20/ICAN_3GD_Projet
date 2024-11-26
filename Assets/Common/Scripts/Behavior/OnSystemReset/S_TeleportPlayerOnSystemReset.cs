using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_PlayerTeleportModule))]
[RequireComponent(typeof(S_SystemResetModule))]
public class S_TeleportPlayerOnSystemReset : MonoBehaviour
{
    public bool useSceneResetManager = true; // Si activé, chercher automatiquement le ResetManager dans la scène

    public S_SystemResetModule systemResetModule;
    private S_PlayerTeleportModule playerTeleportModule;

    private void Awake()
    {
        if (useSceneResetManager)
        {
            // Chercher automatiquement le ResetManager dans la scène si nécessaire
            systemResetModule = FindObjectOfType<S_SystemResetModule>();
        }
        else
        {
            // Utilisateur doit fournir une référence au composant S_PlayerResetModule
            systemResetModule = GetComponent<S_SystemResetModule>();
        }
        // Référencer S_PlayerTeleportModule
        playerTeleportModule = GetComponent<S_PlayerTeleportModule>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // Écouter l'événement PlayerResetEvent du module de réinitialisation du joueur
            systemResetModule.SystemResetEvent += HandlePlayerReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'écoute de l'événement PlayerResetEvent
            systemResetModule.SystemResetEvent -= HandlePlayerReset;
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
