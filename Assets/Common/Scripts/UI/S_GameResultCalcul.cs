using System;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Serialization;

public class S_GameResultCalcul : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settlementPanel;
    
    // Combine kills and score into one Text per entry
    public TMP_Text[] entryTexts;        // length should match number of ScoreData entries

    // Combine time label and bonus into a single Text
    public TMP_Text timeBonusText;     
    public TMP_Text timeResultText;     

    // Display the final total score with bonus applied
    public TMP_Text finalScoreText;

    [Header("Data Sources")]
    public S_ScoreManager scoreManager;
    public S_MainObjective mainObjective;

    [Header("Time Bonus Mapping (in minutes)")]
    [Tooltip("Time range in minutes (min, max)")]
    public Vector2 timeRangeMinutes = new Vector2(0, 5);
    [Tooltip("Bonus percent range corresponding to time range")]
    public Vector2 bonusPercentRange = new Vector2(5, 25);


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ShowResultScreen();
        }
    }

    /// <summary>
    /// Call this to show the settlement screen and populate all fields.
    /// </summary>
    public void ShowResultScreen()
    {
        // 1. Pause the game
        Time.timeScale = 0f;

        // 2. Activate the settlement UI
        settlementPanel.SetActive(true);

        // 3. Populate each entry with 2 lines: Kills and Score
        var dataList = scoreManager.scoreDatas;
        int count = Mathf.Min(dataList.Count, entryTexts.Length);

        // Calculate bonus percentage based on play time
        float totalSeconds = mainObjective.gameChrono;
        float totalMinutes = totalSeconds / 60f;
        float bonusPercent = MapValueClamped(
            totalMinutes,
            timeRangeMinutes.x, timeRangeMinutes.y,
            bonusPercentRange.x, bonusPercentRange.y
        );
        float bonusMultiplier = 1 + bonusPercent / 100f;

        for (int i = 0; i < count; i++)
        {
            var data = dataList[i];
            
            // Kills line: "Kills {count}"
            string killsLine = $"Kills :  {data.killed}";

            // Score line: "Score {adjustedScore}"
            int adjustedScore = Mathf.RoundToInt(data.totalScore);
            string scoreLine = $"Score :  {adjustedScore}";

            entryTexts[i].text = $"{killsLine}\n{scoreLine}";
        }

        // 4. Build time summary: "Time {MM:SS} +XX%"
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        string timeText = $"{minutes:D2}:{seconds:D2}";
        string bonusText = $"x {bonusPercent:F0}%";
        timeResultText.text = $"Time: {timeText}";
        timeBonusText.text = $"{bonusText}";

        // 5. Calculate and display final total score
        float sumOriginal = dataList.Sum(d => d.totalScore);
        int sumAdjusted = Mathf.RoundToInt(sumOriginal * bonusMultiplier);
        finalScoreText.text = sumAdjusted.ToString();
    }

    /// <summary>
    /// Linearly maps a value from [inMin,inMax] to [outMin,outMax], clamped.
    /// </summary>
    private float MapValueClamped(
        float value,
        float inMin, float inMax,
        float outMin, float outMax)
    {
        float t = Mathf.InverseLerp(inMin, inMax, value);
        return Mathf.Lerp(outMin, outMax, t);
    }
}
