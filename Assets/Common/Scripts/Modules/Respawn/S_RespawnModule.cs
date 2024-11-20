using System;
using System.Collections;
using UnityEngine;

public class S_RespawnModule : MonoBehaviour
{
    public int maxRespawnCount = -1; // -1 signifie un nombre infini de respawns
    public float respawnTime = -1f; // Temps de respawn en secondes (-1 signifie pas de d�lai de respawn)
    public int respawnWithSysResetCount = -1; // -1 signifie que ce module ne d�pend pas du reset syst�me
    public int respawnWithPlayerResetCount = -1; // -1 signifie que ce module ne d�pend pas du reset joueur
    public bool respawnOriginalPosition = true; // R�appara�tre � la position d'origine ?
    public GameObject resetManager; // R�f�rence au Reset Manager pour obtenir les compteurs

    public event Action OnRespawn; // �v�nement d�clench� lors du respawn

    private Vector3 originalPosition; // Position d'origine de l'objet
    private bool isRespawning = false; // Emp�che le respawn multiple simultan�
    private int currentRespawnCount; // Compteur des respawns restants

    private S_PlayerResetCounterModule playerResetCounter; // R�f�rence au compteur de reset du joueur
    private S_SystemResetCounterModule systemResetCounter; // R�f�rence au compteur de reset syst�me

    private int initialPlayerResetCount; // Compteur initial de reset du joueur
    private int initialSystemResetCount; // Compteur initial de reset syst�me

    private void Start()
    {
        // Chercher automatiquement le Reset Manager si non attribu�
        if (resetManager == null)
        {
            resetManager = GameObject.Find("ResetManager");
        }
        // Initialisation au d�marrage
        originalPosition = transform.position;
        currentRespawnCount = maxRespawnCount;

        // Obtenir les r�f�rences des compteurs de reset
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

            StartCoroutine(CheckForRespawnCondition()); // Commencer � v�rifier les conditions de respawn
        }
    }

    private IEnumerator CheckForRespawnCondition()
    {
        while (!isRespawning)
        {
            // V�rifier si les conditions de respawn sont remplies
            if ((respawnWithSysResetCount != -1 && ShouldRespawnWithSystemReset()) ||
                (respawnWithPlayerResetCount != -1 && ShouldRespawnWithPlayerReset()))
            {
                isRespawning = true;
                StartCoroutine(RespawnCoroutine());
            }
            yield return null; // Attendre la prochaine frame avant de v�rifier � nouveau
        }
    }

    private bool ShouldRespawnWithSystemReset()
    {
        // V�rifier si le nombre de resets syst�me atteint le seuil pour le respawn
        if (systemResetCounter != null && (systemResetCounter.SystemResetCount - initialSystemResetCount) >= respawnWithSysResetCount)
        {
            return true;
        }
        return false;
    }

    private bool ShouldRespawnWithPlayerReset()
    {
        // V�rifier si le nombre de resets joueur atteint le seuil pour le respawn
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

        // R�appara�tre � la position d'origine ou � la position actuelle
        if (respawnOriginalPosition)
        {
            transform.position = originalPosition;
        }

        // R�activer les composants visuels, de collision, et physiques
        EnableObject();

        // D�cr�menter le compteur de respawn si n�cessaire
        if (maxRespawnCount > 0)
        {
            currentRespawnCount--;
        }

        // D�clencher l'�v�nement de respawn
        OnRespawn?.Invoke();

        isRespawning = false; // Marquer le respawn comme termin�
    }

    private void EnableObject()
    {
        // R�activer les composants visuels, de collision, et physiques
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = true;

        Collider collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
    }
}
