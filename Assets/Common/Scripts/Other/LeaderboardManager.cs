using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject userMarkPrefab; // prefab de UserMark
    [SerializeField] private Transform scrollContent; // parent des entrées (Scroll/Content par exemple)
    
    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

    public void AddEntry(string name, float time, int score)
    {
        leaderboard.Add(new LeaderboardEntry
        {
            playerName = name,
            time = time,
            score = score
        });

        leaderboard = leaderboard.OrderByDescending(e => e.score).ThenBy(e => e.time).ToList();
        RefreshUI();
    }

    private void RefreshUI()
    {
        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < leaderboard.Count; i++)
        {
            GameObject entryGO = Instantiate(userMarkPrefab, scrollContent);
            UserMark mark = entryGO.GetComponent<UserMark>();
            mark.SetValues(i + 1, leaderboard[i].playerName, leaderboard[i].time, leaderboard[i].score);
        }
    }
}

