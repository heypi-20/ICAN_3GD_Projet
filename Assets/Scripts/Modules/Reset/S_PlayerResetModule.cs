using System;
using System.Collections;
using UnityEngine;

public class S_PlayerResetModule : MonoBehaviour
{
    public KeyCode resetKey = KeyCode.R; // Touche de reset, peut �tre assign�e � n'importe quelle touche
    public int maxResetCount = 3; // Nombre maximal de resets possibles
    public float resetCooldown = 5f; // Temps de recharge entre chaque reset en secondes
    public bool allowResetRecovery = true; // Autoriser la r�cup�ration des resets ?
    public float recoveryTime = 60f; // Temps avant la r�cup�ration d'un reset, ignor� si allowResetRecovery est faux

    public event Action PlayerResetEvent; // �v�nement d�clench� lors du reset

    private int currentResetCount; // Nombre de resets restants
    private bool isInCooldown = false; // Indicateur de si le reset est en cooldown
    private bool isRecovering = false; // Indicateur de si une r�cup�ration est en cours

    private void Start()
    {
        // Initialiser le compteur de reset avec le nombre maximal
        currentResetCount = maxResetCount;
    }

    private void Update()
    {
        // V�rifier si la touche de reset est press�e et si un reset est possible
        if (Input.GetKeyDown(resetKey) && currentResetCount > 0 && !isInCooldown)
        {
            // Ex�cuter le reset
            PerformReset();
        }
    }

    private void PerformReset()
    {
        // D�cr�menter le compteur de reset
        currentResetCount--;

        // Lancer le cooldown du reset
        StartCoroutine(ResetCooldownCoroutine());

        // Si les resets sont � z�ro et que la r�cup�ration est autoris�e, commencer la r�cup�ration
        if (currentResetCount == 0 && allowResetRecovery && !isRecovering)
        {
            StartCoroutine(ResetRecoveryCoroutine());
        }

        // D�clencher l'�v�nement de reset
        InvokePlayerResetEvent();

        // Logique suppl�mentaire pour le reset peut �tre ajout�e ici
        Debug.Log("Player reset performed.");
    }

    public void InvokePlayerResetEvent()
    {
        // Invoquer l'�v�nement de reset du joueur
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
        // Commencer la r�cup�ration du reset
        isRecovering = true;
        yield return new WaitForSeconds(recoveryTime);

        // R�cup�rer un reset si la r�cup�ration est autoris�e
        if (allowResetRecovery)
        {
            currentResetCount = maxResetCount;
            Debug.Log("Player reset count recovered.");
        }

        isRecovering = false; // Fin de la r�cup�ration
    }
}
