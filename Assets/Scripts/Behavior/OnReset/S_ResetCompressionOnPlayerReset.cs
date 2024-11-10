using UnityEngine;
using System;

[RequireComponent(typeof(S_PlayerResetModule))]
[RequireComponent(typeof(S_PlayerResetCompressionModule))]
public class S_ResetCompressionOnPlayerReset : MonoBehaviour
{
    private S_PlayerResetModule playerResetModule;
    private S_PlayerResetCompressionModule resetCompressionModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PlayerResetModule et S_PlayerResetCompressionModule
        playerResetModule = GetComponent<S_PlayerResetModule>();
        resetCompressionModule = GetComponent<S_PlayerResetCompressionModule>();
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
        Debug.Log("Événement PlayerReset détecté. Lancer la compression de réinitialisation...");

        if (resetCompressionModule != null)
        {
            // Appeler la méthode CompressReset du module de compression de réinitialisation
            resetCompressionModule.CompressReset();
        }
        else
        {
            Debug.LogWarning("Le module de compression de réinitialisation n'est pas correctement assigné.");
        }
    }
}
