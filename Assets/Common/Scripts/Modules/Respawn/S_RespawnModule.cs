using System;
using System.Collections;
using UnityEngine;

public class S_RespawnModule : MonoBehaviour
{
    public int maxRespawnCount = -1; // -1 signifie un nombre infini de respawns
    public float respawnTime = -1f; // Temps de respawn en secondes (-1 signifie pas de délai de respawn)
    public int respawnWithSysResetCount = -1; // -1 signifie que ce module ne dépend pas du reset système
    public int respawnWithPlayerResetCount = -1; // -1 signifie que ce module ne dépend pas du reset joueur
    public bool respawnOriginalPosition = true; // Réapparaître à la position d'origine ?
    public GameObject resetManager; // Référence au Reset Manager pour obtenir les compteurs

    public event Action OnRespawn; // Événement déclenché lors du respawn

    private Vector3 originalPosition; // Position d'origine de l'objet
    private bool isRespawning = false; // Empêche le respawn multiple simultané
    private int currentRespawnCount; // Compteur des respawns restants

    private S_PlayerResetCounterModule playerResetCounter; // Référence au compteur de reset du joueur
    private S_SystemResetCounterModule systemResetCounter; // Référence au compteur de reset système

    private int initialPlayerResetCount; // Compteur initial de reset du joueur
    private int initialSystemResetCount; // Compteur initial de reset système

    private void Start()
    {
        // Chercher automatiquement le Reset Manager si non attribué
        if (resetManager == null)
        {
            resetManager = GameObject.Find("ResetManager");
        }
        // Initialisation au démarrage
        originalPosition = transform.position;
        currentRespawnCount = maxRespawnCount;

        // Obtenir les références des compteurs de reset
        if (resetManager != null)
        {
            playerResetCounter = resetManager.GetComponent<S_PlayerResetCounterModule>();
            systemResetCounter = resetManager.GetComponent<S_SystemResetCounterModule>();
        }
    }

    public void LaunchRespawnProcess()
    {
        if (!isRespawning && (currentRespawnCount > 0 || maxRespawnCount == -1))
        {
            // Initialiser les compteurs de reset initiaux
            if (playerResetCounter != null)
            {
                initialPlayerResetCount = playerResetCounter.PlayerResetCount;
            }
            if (systemResetCounter != null)
            {
                initialSystemResetCount = systemResetCounter.SystemResetCount;
            }

            StartCoroutine(CheckForRespawnCondition()); // Commencer à vérifier les conditions de respawn
        }
    }

    private IEnumerator CheckForRespawnCondition()
    {
        while (!isRespawning)
        {
            // Vérifier si les conditions de respawn sont remplies
            if ((respawnWithSysResetCount != -1 && ShouldRespawnWithSystemReset()) ||
                (respawnWithPlayerResetCount != -1 && ShouldRespawnWithPlayerReset()))
            {
                isRespawning = true;
                StartCoroutine(RespawnCoroutine());
            }
            yield return null; // Attendre la prochaine frame avant de vérifier à nouveau
        }
    }

    private bool ShouldRespawnWithSystemReset()
    {
        // Vérifier si le nombre de resets système atteint le seuil pour le respawn
        if (systemResetCounter != null && (systemResetCounter.SystemResetCount - initialSystemResetCount) >= respawnWithSysResetCount)
        {
            return true;
        }
        return false;
    }

    private bool ShouldRespawnWithPlayerReset()
    {
        // Vérifier si le nombre de resets joueur atteint le seuil pour le respawn
        if (playerResetCounter != null && (playerResetCounter.PlayerResetCount - initialPlayerResetCount) >= respawnWithPlayerResetCount)
        {
            return true;
        }
        return false;
    }

    private IEnumerator RespawnCoroutine()
    {
        if (respawnTime != -1)
        {
            // Attendre le temps de respawn
            yield return new WaitForSeconds(respawnTime);
        }

        // Réapparaître à la position d'origine ou à la position actuelle
        if (respawnOriginalPosition)
        {
            transform.position = originalPosition;
        }

        // Réactiver les composants visuels, de collision, et physiques
        EnableObject();

        // Décrémenter le compteur de respawn si nécessaire
        if (maxRespawnCount > 0)
        {
            currentRespawnCount--;
        }

        // Déclencher l'événement de respawn
        OnRespawn?.Invoke();

        isRespawning = false; // Marquer le respawn comme terminé
    }

    private void EnableObject()
    {
        // Réactiver les composants visuels, de collision, et physiques
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}
