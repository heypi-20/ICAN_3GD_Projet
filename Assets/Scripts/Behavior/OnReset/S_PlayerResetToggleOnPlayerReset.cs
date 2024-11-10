using UnityEngine;
using System;

[RequireComponent(typeof(S_PlayerResetModule))]
[RequireComponent(typeof(S_PlayerToggleModules))]
public class S_PlayerResetToggleOnPlayerReset : MonoBehaviour
{
    private S_PlayerResetModule playerResetModule;
    private S_PlayerToggleModules playerToggleModules;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_PlayerResetModule et S_PlayerToggleModules
        playerResetModule = GetComponent<S_PlayerResetModule>();
        playerToggleModules = GetComponent<S_PlayerToggleModules>();
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
        Debug.Log("Événement PlayerReset détecté. Incrémenter le compteur de réinitialisations...");

        if (playerToggleModules != null)
        {
            // Appeler la méthode IncrementResetCount du module de basculement
            playerToggleModules.IncrementResetCount();
        }
        else
        {
            Debug.LogWarning("Le module de basculement n'est pas correctement assigné.");
        }
    }
}
