using UnityEngine;
using System;

[RequireComponent(typeof(S_SystemResetModule))]
[RequireComponent(typeof(S_SystemResetSpawnModule))]
public class S_SystemSpawnOnSystemReset : MonoBehaviour
{
    private S_SystemResetModule systemResetModule;
    private S_SystemResetSpawnModule systemResetSpawnModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SystemResetModule et S_SystemResetSpawnModule
        systemResetModule = GetComponent<S_SystemResetModule>();
        systemResetSpawnModule = GetComponent<S_SystemResetSpawnModule>();
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
        Debug.Log("Événement SystemReset détecté. Lancer l'apparition des objets...");

        if (systemResetSpawnModule != null)
        {
            // Appeler la méthode SpawnObjects du module de spawn
            systemResetSpawnModule.SystemSpawnObjects();
        }
        else
        {
            Debug.LogWarning("Le module de spawn du système n'est pas correctement assigné.");
        }
    }
}
