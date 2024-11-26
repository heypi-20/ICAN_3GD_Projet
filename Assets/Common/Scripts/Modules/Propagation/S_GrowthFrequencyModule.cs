using System;
using UnityEngine;
using System.Collections;

public class S_GrowthFrequencyModule : MonoBehaviour
{
    public float growingEveryXTime = -1f; // Temps de croissance en secondes (-1 = pas de croissance par temps)
    public int growingEveryXCalls = -1; // Nombre d'appels pour la croissance (-1 = pas de croissance par appels)
    public int growingCount = 1; // Nombre de croissances par appel

    public event Action GrowthRequest; // Événement pour demander la croissance

    private int currentCallCount = 0; // Compteur d'appels
    private bool isWaitingForTime = false; // Indicateur pour savoir si le module attend la fin du délai de croissance
    private bool SeedIsActive = false;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            UnlockGrowth();
        }
    }

    public void UnlockGrowth()
    {
        Debug.Log("UnlockGrowthRequest");
        SeedIsActive = true;
        StartGrowth();
    }
    public void LockGrowth()
    {
        SeedIsActive = false;
        StopGrowthByTime();
    }

    public void StartGrowth()
    {
        if (growingEveryXCalls > -1 && !isWaitingForTime && SeedIsActive)
        {
            StartGrowthByCall();
        }
        else if (growingEveryXTime > -1 && SeedIsActive)
        {
            Debug.Log("GrowthRequest");
            InvokeRepeating(nameof(GrowthByTime), growingEveryXTime, growingEveryXTime);
        }
    }

    public void StopGrowthByTime()
    {
        CancelInvoke(nameof(GrowthByTime));
        Debug.Log("Growth process stopped.");
    }

    private void StartGrowthByCall()
    {
        currentCallCount++;
        if (currentCallCount >= growingEveryXCalls && growingEveryXCalls > -1)
        {
            currentCallCount = 0;
            if (growingEveryXTime > -1)
            {
                isWaitingForTime = true;
                StartCoroutine(WaitForGrowthTime());
            }
            else
            {
                StartCoroutine(InvokeGrowthRequest(growingCount));
            }
        }
    }

    private IEnumerator WaitForGrowthTime()
    {
        yield return new WaitForSeconds(growingEveryXTime);
        
        StartCoroutine(InvokeGrowthRequest(growingCount));
        isWaitingForTime = false;
    }

    public void InvokeGrowthRequestEvent()
    {
        Debug.Log("GrowthRequest event invoked");
        GrowthRequest?.Invoke();
    }

    private void GrowthByTime()
    {
        StartCoroutine(InvokeGrowthRequest(growingCount));
    }

    private IEnumerator InvokeGrowthRequest(int count)
    {
        int growthCompleted = 0;

        while (growthCompleted < count)
        {
            InvokeGrowthRequestEvent();
            growthCompleted++;
            yield return null; // Attendre une frame pour éviter de bloquer Unity
        }
    }
}
