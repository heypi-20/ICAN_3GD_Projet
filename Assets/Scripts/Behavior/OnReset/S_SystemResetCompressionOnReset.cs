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
        // Ajouter automatiquement et r�f�rencer S_SystemResetModule et S_SystemResetCompressionModule
        systemResetModule = GetComponent<S_SystemResetModule>();
        resetCompressionModule = GetComponent<S_SystemResetCompressionModule>();
    }

    private void OnEnable()
    {
        if (systemResetModule != null)
        {
            // �couter l'�v�nement OnReset du module de r�initialisation du syst�me
            systemResetModule.SystemeResetEvent += HandleSystemReset;
        }
    }

    private void OnDisable()
    {
        if (systemResetModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnReset
            systemResetModule.SystemeResetEvent -= HandleSystemReset;
        }
    }

    private void HandleSystemReset()
    {
        Debug.Log("�v�nement SystemReset d�tect�. Lancer la compression de r�initialisation du syst�me...");

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
