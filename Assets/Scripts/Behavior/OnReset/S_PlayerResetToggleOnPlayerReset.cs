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
        // Ajouter automatiquement et r�f�rencer S_PlayerResetModule et S_PlayerToggleModules
        playerResetModule = GetComponent<S_PlayerResetModule>();
        playerToggleModules = GetComponent<S_PlayerToggleModules>();
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
        Debug.Log("�v�nement PlayerReset d�tect�. Incr�menter le compteur de r�initialisations...");

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
