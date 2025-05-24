using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Downloads the leaderboard from Dreamlo, populates the Scroll-View,
/// highlights the local player, and scrolls the list to the relevant row.
/// </summary>
public class S_LeaderboardDisplay : MonoBehaviour
{
    [Header("References")]
    public S_DreamloManager dreamlo;     // Component that handles Dreamlo API calls
    public GameObject entryPrefab;       // Prefab containing RankText / NameText / ScoreText / TimeText
    public Transform contentContainer;   // ScrollView/Viewport/Content transform
    public ScrollRect scrollRect;        // ScrollRect component that controls scrolling

    [Header("Settings")]
    public Color highlightColor = Color.yellow; // Text colour used to highlight the local player

    private string localPlayerName;

    void Start()
    {
        // Load player name from PlayerPrefs (empty string if not set)
        localPlayerName = PlayerPrefs.GetString("PlayerName", string.Empty);

        // Pull the top 100 scores from Dreamlo
        StartCoroutine(dreamlo.DownloadTopScores(100, OnLeaderboardReceived));
    }

    /// <summary>
    /// Callback executed when the list is downloaded.
    /// Generates UI rows, highlights the local player and starts the scroll coroutine.
    /// </summary>
    void OnLeaderboardReceived(List<Entry> entries)
    {
        int playerIndex = -1;

        // 1. Destroy any old rows
        foreach (Transform child in contentContainer)
            Destroy(child.gameObject);

        // 2. Instantiate a row for every entry
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            GameObject row = Instantiate(entryPrefab, contentContainer);
            row.transform.localScale = Vector3.one;

            // Fill the row’s text fields
            foreach (TMP_Text txt in row.GetComponentsInChildren<TMP_Text>())
            {
                switch (txt.name)
                {
                    case "RankText":  txt.text = entry.rank.ToString();          break;
                    case "NameText":  txt.text = entry.name;                     break;
                    case "ScoreText": txt.text = entry.score.ToString();         break;
                    case "TimeText":
                        int m = entry.seconds / 60;
                        int s = entry.seconds % 60;
                        txt.text = $"{m:D2}:{s:D2}";
                        break;
                }
            }

            // Check if this is the local player
            if (entry.name == localPlayerName)
            {
                playerIndex = i;

                // Highlight every text component in the row
                foreach (TMP_Text txt in row.GetComponentsInChildren<TMP_Text>())
                    txt.color = highlightColor;
            }
        }

        // 3. Smooth-scroll to the local player row (or to the top if not found)
        var contentRect = contentContainer.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();

        float targetY;
        if (playerIndex >= 0)
        {
            float total = contentContainer.childCount;
            targetY = 1f - (playerIndex / (total - 1f));
            targetY = Mathf.Clamp(targetY, 0f, 1f);
        }
        else
        {
            targetY = 1f;
        }
        scrollRect.verticalNormalizedPosition = targetY;
    }
}
