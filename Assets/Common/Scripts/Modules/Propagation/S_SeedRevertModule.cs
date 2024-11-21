using System;
using System.Collections;
using UnityEngine;
using VFolders.Libs;

public class S_SeedRevertModule : MonoBehaviour
{
    public int minChildCount = 1; // Nombre minimum d'enfants avant de déclencher le retour à la forme de graine
    public float checkInterval = 1.0f; // Intervalle de temps pour vérifier le nombre d'enfants
    public event Action OnRevertToSeed;

   

    public void StartCheckingChildCount()
    {
        StartCoroutine(CheckChildCountRoutine());
    }

    private IEnumerator CheckChildCountRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckChildCount();
        }
    }

    private void CheckChildCount()
    {
       
        if (transform.childCount < minChildCount)
        {
            TriggerRevertToSeed();
        }
    }

    private void TriggerRevertToSeed()
    {
        
        Debug.Log("Stop");
        OnRevertToSeed?.Invoke();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject, 0.01f); // Détruire tous les enfants de l'objet avec un délai pour minimiser les coûts de performance
        }
        StopAllCoroutines(); // Arrêter toutes les coroutines pour empêcher une nouvelle vérification
    }
}
