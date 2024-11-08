using System;
using System.Collections;
using UnityEngine;

public class S_PlayerResetModule : MonoBehaviour
{
    public KeyCode resetKey = KeyCode.R; // Touche de reset, peut être assignée à n'importe quelle touche
    public int maxResetCount = 3; // Nombre maximal de resets possibles
    public float resetCooldown = 5f; // Temps de recharge entre chaque reset en secondes
    public bool allowResetRecovery = true; // Autoriser la récupération des resets ?
    public float recoveryTime = 60f; // Temps avant la récupération d'un reset, ignoré si allowResetRecovery est faux

    public event Action PlayerResetEvent; // Événement déclenché lors du reset

    private int currentResetCount; // Nombre de resets restants
    private bool isInCooldown = false; // Indicateur de si le reset est en cooldown
    private bool isRecovering = false; // Indicateur de si une récupération est en cours

    private void Start()
    {
        // Initialiser le compteur de reset avec le nombre maximal
        currentResetCount = maxResetCount;
    }

    private void Update()
    {
        // Vérifier si la touche de reset est pressée et si un reset est possible
        if (Input.GetKeyDown(resetKey) && currentResetCount > 0 && !isInCooldown)
        {
            // Exécuter le reset
            PerformReset();
        }
    }

    private void PerformReset()
    {
        // Décrémenter le compteur de reset
        currentResetCount--;

        // Lancer le cooldown du reset
        StartCoroutine(ResetCooldownCoroutine());

        // Si les resets sont à zéro et que la récupération est autorisée, commencer la récupération
        if (currentResetCount == 0 && allowResetRecovery && !isRecovering)
        {
            StartCoroutine(ResetRecoveryCoroutine());
        }

        // Déclencher l'événement de reset
        InvokePlayerResetEvent();

        // Logique supplémentaire pour le reset peut être ajoutée ici
        Debug.Log("Player reset performed.");
    }

    public void InvokePlayerResetEvent()
    {
        // Invoquer l'événement de reset du joueur
        PlayerResetEvent?.Invoke();
    }

    private IEnumerator ResetCooldownCoroutine()
    {
        // Commencer le cooldown du reset
        isInCooldown = true;
        yield return new WaitForSeconds(resetCooldown);
        isInCooldown = false; // Fin du cooldown
    }

    private IEnumerator ResetRecoveryCoroutine()
    {
        // Commencer la récupération du reset
        isRecovering = true;
        yield return new WaitForSeconds(recoveryTime);

        // Récupérer un reset si la récupération est autorisée
        if (allowResetRecovery)
        {
            currentResetCount = maxResetCount;
            Debug.Log("Player reset count recovered.");
        }

        isRecovering = false; // Fin de la récupération
    }
}
