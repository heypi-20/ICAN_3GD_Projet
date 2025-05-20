using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ScoreData
{
    public EnemyType enemyType;
    public int scoreGiven;
    [HideInInspector] public int killed = 0;
}

public class S_ScoreManager : MonoBehaviour
{
    public List<ScoreData> scoreDatas;
    
    [Header("Read Only Global Score")]
    public int score;

    private void OnEnable()
    {
        EnemyBase.OnEnemyKilled += AddScore;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyKilled -= AddScore;
    }

    private void AddScore(EnemyType enemyType)
    {
        foreach (ScoreData scoreData in scoreDatas) {
            if (enemyType == scoreData.enemyType) {
                score += scoreData.scoreGiven;
                scoreData.killed++;
                Debug.Log("Current Killed " +  scoreData.enemyType + " : " + scoreData.killed);
                break;
            }
        }
        
        Debug.Log("Actual Score : " + score);
    }
}

