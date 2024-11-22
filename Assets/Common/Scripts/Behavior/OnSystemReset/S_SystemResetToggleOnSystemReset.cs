using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_SystemResetModule))]
[RequireComponent(typeof(S_SystemToggleModules))]
public class S_SystemResetToggleOnSystemReset : MonoBehaviour
{
    private S_SystemResetModule systemResetModule;
    private S_SystemToggleModules playerToggleModules;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SystemResetModule et S_PlayerToggleModules
        systemResetModule = GetComponent<S_SystemResetModule>();
        playerToggleModules = GetComponent<S_SystemToggleModules>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // Écouter l'événement SystemResetEvent du module de réinitialisation du système
            systemResetModule.SystemResetEvent += HandleSystemReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'écoute de l'événement SystemResetEvent
            systemResetModule.SystemResetEvent -= HandleSystemReset;
        }
    }

    private void HandleSystemReset()
    {
        Debug.Log("Événement SystemReset détecté. Incrémenter le compteur de réinitialisations...");

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
