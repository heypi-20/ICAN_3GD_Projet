using UnityEngine;
using System;

[RequireComponent(typeof(S_DestructionModule))]
[RequireComponent(typeof(S_RespawnModule))]
public class S_RespawnOnDestroyed : MonoBehaviour
{
    private S_DestructionModule destructionModule;
    private S_RespawnModule respawnModule;

    private void Awake()
    {
        // Ajouter automatiquement et r�f�rencer S_DestructionModule et S_RespawnModule
        destructionModule = GetComponent<S_DestructionModule>();
        respawnModule = GetComponent<S_RespawnModule>();
    }

    private void OnEnable()
    {
        if (destructionModule != null)
        {
            // �couter l'�v�nement OnDestroyed du module de destruction
            destructionModule.OnDestroyed += HandleOnDestroyed;
        }
    }

    private void OnDisable()
    {
        if (destructionModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnDestroyed
            destructionModule.OnDestroyed -= HandleOnDestroyed;
        }
    }

    private void HandleOnDestroyed()
    {

        if (respawnModule != null)
        {
            // Appeler la m�thode LaunchRespawnProcess du module de respawn
            respawnModule.LaunchRespawnProcess();
        }
        else
        {
            Debug.LogWarning("Le module de respawn n'est pas correctement assign�.");
        }
    }
}
