using System.Collections;
using UnityEngine;
using TMPro;   // TMP_InputField

/// <summary>
/// Retrieves the player name, score and time from UI / game-logic scripts
/// and uploads the result to Dreamlo when the “Next” button is pressed.
/// </summary>
public class S_GameResultUploader : MonoBehaviour
{
    [Header("References")]
    public S_DreamloManager  dreamlo;      // Drag the DreamloManager in the Inspector
    public S_GameResultCalcul resultCalc;  // Your score-calculation script
    public TMP_InputField    nameInputField; // UI field where the player types their name

    /// <summary>
    /// Hook this to the Button’s OnClick() event in the Inspector.
    /// </summary>
    public void OnClickNext()
    {
        StartCoroutine(UploadCurrentResult());
    }

    /// <summary>
    /// Builds the upload URL and sends the data to Dreamlo.
    /// </summary>
    private IEnumerator UploadCurrentResult()
    {
        // 1. Player name (fall back to “Anonymous”)
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        // 2. Fetch score and elapsed time from your result-calculator
        int   finalScore   = resultCalc.finalScore;
        float totalSeconds = resultCalc.timeChrono;

        // 3. Upload to Dreamlo
        yield return StartCoroutine(
            dreamlo.UploadScore(
                playerName,
                finalScore,
                Mathf.RoundToInt(totalSeconds))
        );
    }
}