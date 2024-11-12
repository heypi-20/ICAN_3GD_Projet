using System;
using System.Collections;
using UnityEngine;

public class S_SystemResetModule : MonoBehaviour
{
    public float resetCountdown = 30f; // Temps avant le déclenchement automatique du reset en secondes
    public int maxSystemResets = -1; // Nombre maximal de resets système (-1 pour un nombre infini)
    public event Action SystemResetEvent; // Événement déclenché lors du reset système

    private bool isCountingDown = false; // Indicateur pour savoir si le compte à rebours est en cours
    private int currentSystemResetCount = 0; // Nombre de resets système effectués

    private void Start()
    {
        // Démarrer le compte à rebours pour le reset système si le temps est supérieur à zéro
        if (resetCountdown > 0)
        {
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        // Commencer le compte à rebours si aucun compte à rebours n'est en cours et si le nombre maximal de resets n'est pas atteint
        if (!isCountingDown && (maxSystemResets == -1 || currentSystemResetCount < maxSystemResets))
        {
            StartCoroutine(ResetCountdownCoroutine());
        }
    }

    private IEnumerator ResetCountdownCoroutine()
    {
        // Démarrer le compte à rebours
        isCountingDown = true;
        yield return new WaitForSeconds(resetCountdown);

        // Déclencher l'événement de reset système
        InvokeSystemeResetEvent();

        // Incrémenter le compteur de resets système
        currentSystemResetCount++;
        isCountingDown = false;

        // Relancer le compte à rebours si le nombre de resets n'est pas atteint et que le temps est défini
        if (resetCountdown > 0 && (maxSystemResets == -1 || currentSystemResetCount < maxSystemResets))
        {
            StartCountdown();
        }
    }

    public void InvokeSystemeResetEvent()
    {
        // Invoquer l'événement de reset système
        Debug.Log("System Reset");
        SystemResetEvent?.Invoke();
    }
}
