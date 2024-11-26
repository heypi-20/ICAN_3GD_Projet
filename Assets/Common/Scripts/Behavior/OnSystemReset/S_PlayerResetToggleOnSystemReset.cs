using UnityEngine;
using System;

[RequireComponent(typeof(S_SystemResetModule))]
[RequireComponent(typeof(S_PlayerToggleModules))]
public class S_PlayerResetToggleOnSystemReset : MonoBehaviour
{
    private S_SystemResetModule systemResetModule;
    private S_PlayerToggleModules playerToggleModules;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_SystemResetModule et S_PlayerToggleModules
        systemResetModule = GetComponent<S_SystemResetModule>();
        playerToggleModules = GetComponent<S_PlayerToggleModules>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // �couter l'�v�nement SystemResetEvent du module de r�initialisation du syst�me
            systemResetModule.SystemResetEvent += HandleSystemReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'�coute de l'�v�nement SystemResetEvent
            systemResetModule.SystemResetEvent -= HandleSystemReset;
        }
    }

    private void HandleSystemReset()
    {
        Debug.Log("�v�nement SystemReset d�tect�. Incr�menter le compteur de r�initialisations...");

        if (playerToggleModules != null)
        {
            // Appeler la m�thode IncrementResetCount du module de basculement
            playerToggleModules.IncrementResetCount();
        }
        else
        {
            Debug.LogWarning("Le module de basculement n'est pas correctement assign�.");
        }
    }
}
