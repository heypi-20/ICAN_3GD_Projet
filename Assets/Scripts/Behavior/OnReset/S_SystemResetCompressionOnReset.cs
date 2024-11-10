using UnityEngine;
using System;

[RequireComponent(typeof(S_SystemResetModule))]
[RequireComponent(typeof(S_SystemResetCompressionModule))]
public class S_SystemResetCompressionOnReset : MonoBehaviour
{
    private S_SystemResetModule systemResetModule;
    private S_SystemResetCompressionModule resetCompressionModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_SystemResetModule et S_SystemResetCompressionModule
        systemResetModule = GetComponent<S_SystemResetModule>();
        resetCompressionModule = GetComponent<S_SystemResetCompressionModule>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // Écouter l'événement OnReset du module de réinitialisation du système
            systemResetModule.SystemeResetEvent += HandleSystemReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'écoute de l'événement OnReset
            systemResetModule.SystemeResetEvent -= HandleSystemReset;
        }
    }

    private void HandleSystemReset()
    {
        Debug.Log("Événement SystemReset détecté. Lancer la compression de réinitialisation du système...");

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
