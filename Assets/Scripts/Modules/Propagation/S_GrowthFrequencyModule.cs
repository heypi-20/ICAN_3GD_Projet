using System;
using UnityEngine;
using System.Collections;

public class S_GrowthFrequencyModule : MonoBehaviour
{

    public float growingEveryXTime = -1f; // Temps de croissance en secondes (-1 = pas de croissance par temps)
    public int growingEveryXCalls = -1; // Nombre d'appels pour la croissance (-1 = pas de croissance par appels)
    public int growingCount = 1; // Nombre de croissances par appel
    public bool growthWithCurve = false; // La croissance suit-elle une courbe ?
    public AnimationCurve growthCurve; // Courbe de croissance
    public float growthDuration = 1f; // Temps nécessaire pour terminer la croissance complète

    public event Action GrowthRequest; // Événement pour demander la croissance

    private int currentCallCount = 0; // Compteur d'appels

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            StartGrowth();
        }
    }

    public void StartGrowth()
    {
        if (growingEveryXTime > -1)
        {
            InvokeRepeating(nameof(GrowthByTime), growingEveryXTime, growingEveryXTime);
            
        }
        else
        {
            StartGrowthByCall();
        }
    }

    private void StartGrowthByCall()
    {
        currentCallCount++;
        if (currentCallCount >= growingEveryXCalls && growingEveryXCalls > -1)
        {
            currentCallCount = 0;
            StartCoroutine(InvokeGrowthRequestWithCurve(growingCount, growthDuration));
            
        }
    }

    public void InvokeGrowthRequestEvent()
    {
        Debug.Log("111111");
        GrowthRequest?.Invoke();
    }

    private void GrowthByTime()
    {
        StartCoroutine(InvokeGrowthRequestWithCurve(growingCount, growthDuration));
    }

    private IEnumerator InvokeGrowthRequestWithCurve(int count, float duration)
    {
        float startTime = Time.time;
        float elapsedTime = 0f;
        int growthCompleted = 0;

        while (growthCompleted < count)
        {
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float rate = growthWithCurve && growthCurve != null ? growthCurve.Evaluate(progress) : 1f;

            int growthToProcess = Mathf.CeilToInt(rate * count / (duration / Time.deltaTime));
            growthToProcess = Mathf.Min(growthToProcess, count - growthCompleted);

            for (int i = 0; i < growthToProcess; i++)
            {
                InvokeGrowthRequestEvent();
                growthCompleted++;
            }

            elapsedTime = Time.time - startTime;
            yield return null; // Attendre une frame pour éviter de bloquer Unity
        }
    }
}
