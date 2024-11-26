using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(S_PlayerTeleportModule))]
[RequireComponent(typeof(S_SystemResetModule))]
public class S_TeleportPlayerOnSystemReset : MonoBehaviour
{
    public bool useSceneResetManager = true; // Si activ�, chercher automatiquement le ResetManager dans la sc�ne

    public S_SystemResetModule systemResetModule;
    private S_PlayerTeleportModule playerTeleportModule;

    private void Awake()
    {
        if (useSceneResetManager)
        {
            // Chercher automatiquement le ResetManager dans la sc�ne si n�cessaire
            systemResetModule = FindObjectOfType<S_SystemResetModule>();
        }
        else
        {
            // Utilisateur doit fournir une r�f�rence au composant S_PlayerResetModule
            systemResetModule = GetComponent<S_SystemResetModule>();
        }
        // R�f�rencer S_PlayerTeleportModule
        playerTeleportModule = GetComponent<S_PlayerTeleportModule>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // �couter l'�v�nement PlayerResetEvent du module de r�initialisation du joueur
            systemResetModule.SystemResetEvent += HandlePlayerReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'�coute de l'�v�nement PlayerResetEvent
            systemResetModule.SystemResetEvent -= HandlePlayerReset;
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
