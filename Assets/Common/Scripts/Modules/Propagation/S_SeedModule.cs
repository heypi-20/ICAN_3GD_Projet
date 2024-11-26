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
    public bool activeKinematic=false;
    private bool seedActived = false; // Le seed est-il d�j� activ� ?
    private int currentCallCount = 0; // Compteur d'appels

    public event Action SeedIsActive; // �v�nement lorsque le seed est activ�
    private Rigidbody seedRb;
    private Renderer seedRenderer; // R�f�rence au Renderer pour changer la couleur

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

    public void ActivateSeed()
    {
        if (seedActived) return; // Si d�j� activ�, ne rien faire
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
        if (!seedActived) return; // Si d�j� d�sactiv�, ne rien faire
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
            // Changer la couleur du seed lui-m�me
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
