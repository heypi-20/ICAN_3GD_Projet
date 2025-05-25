using System;
using UnityEngine;
using TMPro;
using System.Linq;

public class S_GameResultCalcul : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settlementPanel;
    
    // Separate lists for kills and score texts
    [Tooltip("Text fields for displaying kill counts")]
    public TMP_Text[] killsTexts;    // should match number of ScoreData entries
    [Tooltip("Text fields for displaying scores")]
    public TMP_Text[] scoreTexts;    // should match number of ScoreData entries

    // Combine time label and bonus into a single Text
    public TMP_Text timeBonusText;     
    public TMP_Text timeResultText;     

    // Display the final total score with bonus applied
    public TMP_Text finalScoreText;

    private S_ScoreManager scoreManager;
    private S_MainObjective mainObjective;

    [Header("Time Bonus Mapping (in minutes)")]
    [Tooltip("Time range in minutes (min, max)")]
    public Vector2 timeRangeMinutes = new Vector2(0, 5);
    [Tooltip("Bonus percent range corresponding to time range")]
    public Vector2 bonusPercentRange = new Vector2(5, 25);

    public float timeChrono;
    public int finalScore;
    
    private void Start()
    {
        scoreManager = FindObjectOfType<S_ScoreManager>();
        mainObjective = FindObjectOfType<S_MainObjective>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 2. Activate the settlement UI
        settlementPanel.SetActive(true);

        // 3. Populate each entry: kills only number, score only number
        var dataList = scoreManager.scoreDatas;
        int count = Mathf.Min(dataList.Count, killsTexts.Length);
        count = Mathf.Min(count, scoreTexts.Length);

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
            
            // Display kills count only
            killsTexts[i].text = data.killed.ToString();

            // Display raw score only, rounded
            int adjustedScore = Mathf.RoundToInt(data.totalScore);
            scoreTexts[i].text = adjustedScore.ToString();
        }

        // 4. Build time summary: "Time {MM:SS} +XX%"
        int minutes = Mathf.FloorToInt(totalSeconds / 60f);
        int seconds = Mathf.FloorToInt(totalSeconds % 60f);
        string timeText = $"{minutes:D2}:{seconds:D2}";
        string bonusText = $"x {bonusPercent:F0}%";
        timeResultText.text = $"Time: {timeText}";
        timeBonusText.text = bonusText;

        // 5. Calculate and display final total score
        float sumOriginal = dataList.Sum(d => d.totalScore);
        int sumAdjusted = Mathf.RoundToInt(sumOriginal * bonusMultiplier);
        finalScoreText.text = sumAdjusted.ToString();
        finalScore=sumAdjusted;
        timeChrono = totalSeconds;
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
