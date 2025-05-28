using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ScoreData
{
    public EnemyType enemyType;
    public float scoreGiven;
    public int killed = 0;
    public float totalScore = 0;
}

public class S_ScoreManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<ScoreData> scoreDatas;

    [Header("Read Only Global Score")]
    public float gainScore;

    // Quick lookup by enemy type
    private Dictionary<EnemyType, ScoreData> _scoreDataDict;
    private S_ComboSystem _comboSystem;
    private S_DisplayGainScore _displayGainScore;

    private void Awake()
    {
        // Build a dictionary for fast access
        _scoreDataDict = scoreDatas.ToDictionary(sd => sd.enemyType);
        _comboSystem = FindObjectOfType<S_ComboSystem>();
        _displayGainScore= FindObjectOfType<S_DisplayGainScore>();

    }

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
        if (_scoreDataDict.TryGetValue(enemyType, out var data))
        {

            // Update per-type stats
            data.killed++;
            gainScore = data.scoreGiven * _comboSystem.currentComboMultiplier;
            _displayGainScore.ShowGainScore(gainScore);
            data.totalScore += data.scoreGiven*_comboSystem.currentComboMultiplier;
        }
        
    }

    /// <summary>
    /// Retrieves the accumulated score for a specific enemy type.
    /// </summary>
    public float GetScoreByType(EnemyType enemyType)
    {
        if (_scoreDataDict.TryGetValue(enemyType, out var data))
            return data.totalScore;

        Debug.LogWarning($"No ScoreData found for enemy type: {enemyType}");
        return 0;
    }
}
