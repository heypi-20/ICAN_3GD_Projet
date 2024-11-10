using System;
using System.Collections;
using UnityEngine;

public class S_SystemResetModule : MonoBehaviour
{
    public float resetCountdown = 30f; // Temps avant le d�clenchement automatique du reset en secondes
    public int maxSystemResets = -1; // Nombre maximal de resets syst�me (-1 pour un nombre infini)
    public event Action SystemResetEvent; // �v�nement d�clench� lors du reset syst�me

    private bool isCountingDown = false; // Indicateur pour savoir si le compte � rebours est en cours
    private int currentSystemResetCount = 0; // Nombre de resets syst�me effectu�s

    private void Start()
    {
        // D�marrer le compte � rebours pour le reset syst�me si le temps est sup�rieur � z�ro
        if (resetCountdown > 0)
        {
            StartCountdown();
        }
    }

    public void StartCountdown()
    {
        // Commencer le compte � rebours si aucun compte � rebours n'est en cours et si le nombre maximal de resets n'est pas atteint
        if (!isCountingDown && (maxSystemResets == -1 || currentSystemResetCount < maxSystemResets))
        {
            StartCoroutine(ResetCountdownCoroutine());
        }
    }

    private IEnumerator ResetCountdownCoroutine()
    {
        // D�marrer le compte � rebours
        isCountingDown = true;
        yield return new WaitForSeconds(resetCountdown);

        // D�clencher l'�v�nement de reset syst�me
        InvokeSystemeResetEvent();

        // Incr�menter le compteur de resets syst�me
        currentSystemResetCount++;
        isCountingDown = false;

        // Relancer le compte � rebours si le nombre de resets n'est pas atteint et que le temps est d�fini
        if (resetCountdown > 0 && (maxSystemResets == -1 || currentSystemResetCount < maxSystemResets))
        {
            StartCountdown();
        }
    }

    public void InvokeSystemeResetEvent()
    {
        // Invoquer l'�v�nement de reset syst�me
        Debug.Log("System Reset");
        SystemResetEvent?.Invoke();
    }
}
