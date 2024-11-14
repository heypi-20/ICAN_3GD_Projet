using System;
using System.Collections;
using UnityEngine;

public class S_SeedModule : MonoBehaviour
{
    public bool seedActiveOnStart = false; // Le seed est-il activé au démarrage ?
    public int seedActiveAfterXCalls = 0; // Activer le seed après x appels
    public float seedActiveAfterXSeconds = -1f; // Activer le seed après x secondes (-1 signifie pas d'utilisation du délai)
    public bool canBePickUp = false; // Le seed peut-il être ramassé ?
    public bool enableColorChange = false; // Activer le changement de couleur selon l'état ?
    public Color activeColor = Color.green; // Couleur lorsque le seed est activé
    public Color inactiveColor = Color.red; // Couleur lorsque le seed n'est pas activé

    private bool seedActived = false; // Le seed est-il déjà activé ?
    private int currentCallCount = 0; // Compteur d'appels

    public event Action SeedIsActive; // Événement lorsque le seed est activé

    private Renderer seedRenderer; // Référence au Renderer pour changer la couleur

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
        if (seedActived) return; // Si déjà activé, ne rien faire

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
        if (seedActived) return; // Si déjà activé, ne rien faire

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
