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
    public bool activeKinematic=false;
    private bool seedActived = false; // Le seed est-il déjà activé ?
    private int currentCallCount = 0; // Compteur d'appels

    public event Action SeedIsActive; // Événement lorsque le seed est activé
    private Rigidbody seedRb;
    private Renderer seedRenderer; // Référence au Renderer pour changer la couleur

    private void Start()
    {
        seedRenderer = GetComponent<Renderer>();
        seedRb = GetComponent<Rigidbody>();
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

    public void ActivateSeed()
    {
        if (seedActived) return; // Si déjà activé, ne rien faire
        if (activeKinematic)
        {
            seedRb.isKinematic = true;
        }
        seedActived = true;
        UpdateSeedColor();
        SeedIsActive?.Invoke();
        Debug.Log("Seed has been activated.");
    }
    public void DeactivateSeed()
    {
        if (!seedActived) return; // Si déjà désactivé, ne rien faire
        if (activeKinematic)
        {
            seedRb.isKinematic = false;
        }
        seedActived = false;
        UpdateSeedColor();
        Debug.Log("Seed has been deactivated.");
    }

    private void UpdateSeedColor()
    {
        if (enableColorChange)
        {
            // Changer la couleur du seed lui-même
            if (seedRenderer != null)
            {
                seedRenderer.material.color = seedActived ? activeColor : inactiveColor;
            }

            // Changer la couleur des enfants
            foreach (Transform child in transform)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    childRenderer.material.color = seedActived ? activeColor : inactiveColor;
                }
            }
        }
    }
}
