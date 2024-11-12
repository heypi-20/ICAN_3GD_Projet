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
        // Ajouter automatiquement et r�f�rencer S_PlayerResetModule et S_PlayerResetCompressionModule
        playerResetModule = GetComponent<S_PlayerResetModule>();
        resetCompressionModule = GetComponent<S_PlayerResetCompressionModule>();
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
        Debug.Log("�v�nement PlayerReset d�tect�. Lancer la compression de r�initialisation...");

        if (resetCompressionModule != null)
        {
            // Appeler la m�thode CompressReset du module de compression de r�initialisation
            resetCompressionModule.CompressReset();
        }
        else
        {
            Debug.LogWarning("Le module de compression de r�initialisation n'est pas correctement assign�.");
        }
    }
}
