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
        // Ajouter automatiquement et r�f�rencer S_SystemResetModule et S_SystemResetSpawnModule
        systemResetModule = GetComponent<S_SystemResetModule>();
        systemResetSpawnModule = GetComponent<S_SystemResetSpawnModule>();
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
        Debug.Log("�v�nement SystemReset d�tect�. Lancer l'apparition des objets...");

        if (systemResetSpawnModule != null)
        {
            // Appeler la m�thode SpawnObjects du module de spawn
            systemResetSpawnModule.SystemSpawnObjects();
        }
        else
        {
            Debug.LogWarning("Le module de spawn du syst�me n'est pas correctement assign�.");
        }
    }
}
