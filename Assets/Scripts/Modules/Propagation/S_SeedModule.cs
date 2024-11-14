using System;
using System.Collections;
using UnityEngine;

public class S_SeedModule : MonoBehaviour
{
    public bool seedActiveOnStart = false; // Le seed est-il activ� au d�marrage ?
    public int seedActiveAfterXCalls = 0; // Activer le seed apr�s x appels
    public float seedActiveAfterXSeconds = -1f; // Activer le seed apr�s x secondes (-1 signifie pas d'utilisation du d�lai)
    public bool canBePickUp = false; // Le seed peut-il �tre ramass� ?
    public bool enableColorChange = false; // Activer le changement de couleur selon l'�tat ?
    public Color activeColor = Color.green; // Couleur lorsque le seed est activ�
    public Color inactiveColor = Color.red; // Couleur lorsque le seed n'est pas activ�

    private bool seedActived = false; // Le seed est-il d�j� activ� ?
    private int currentCallCount = 0; // Compteur d'appels

    public event Action SeedIsActive; // �v�nement lorsque le seed est activ�

    private Renderer seedRenderer; // R�f�rence au Renderer pour changer la couleur

    private void Start()
    {
        seedRenderer = GetComponent<Renderer>();
        UpdateSeedColor();

        if (seedActiveOnStart)
        {
            ActivateSeed();
        }
        else if (seedActiveAfterXSeconds > 0)
        {
            StartCoroutine(ActivateSeedAfterSeconds(seedActiveAfterXSeconds));
        }
    }

    public void IncrementCallCount()
    {
        if (seedActived) return; // Si d�j� activ�, ne rien faire

        currentCallCount++;
        if (currentCallCount >= seedActiveAfterXCalls)
        {
            ActivateSeed();
        }
    }

    private IEnumerator ActivateSeedAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ActivateSeed();
    }

    private void ActivateSeed()
    {
        if (seedActived) return; // Si d�j� activ�, ne rien faire

        seedActived = true;
        UpdateSeedColor();
        SeedIsActive?.Invoke();
        Debug.Log("Seed has been activated.");
    }

    private void UpdateSeedColor()
    {
        if (enableColorChange && seedRenderer != null)
        {
            seedRenderer.material.color = seedActived ? activeColor : inactiveColor;
        }
    }
}
