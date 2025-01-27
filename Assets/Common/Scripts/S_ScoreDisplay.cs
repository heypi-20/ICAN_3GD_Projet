using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class S_ScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI  scoreText; // The UI text that displays the score

    [Header("Score Settings")]
    public float score = 0f; // The current score
    public float scoreDecayRate = 5f; // Points lost per second
    public float decayDelay = 3f; // Time in seconds before score starts to decay

    private float lastScoreIncreaseTime; // Time of the last score increase

    void Start()
    {
        // Initialize the score text
        UpdateScoreText();
        lastScoreIncreaseTime = Time.time;
    }

    void Update()
    {
        // If the time since the last increase exceeds the delay, start decaying the score
        if (Time.time - lastScoreIncreaseTime > decayDelay)
        {
            ReduceScoreOverTime();
        }
    }

    // Method to add points to the score
    public void AddScore(float points)
    {
        score += points; // Add points to the score
        lastScoreIncreaseTime = Time.time; // Reset the last increase time
        UpdateScoreText(); // Update the display
    }

    // Reduce the score over time
    private void ReduceScoreOverTime()
    {
        score -= scoreDecayRate * Time.deltaTime; // Gradually reduce the score
        score = Mathf.Max(0, score); // Prevent the score from going negative
        UpdateScoreText(); // Update the display
    }

    // Update the score text in the UI
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + Mathf.FloorToInt(score); // Display the score as an integer
    }
}