using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles all Dreamlo API traffic: upload a score and download the top-N list.
/// </summary>
public class S_DreamloManager : MonoBehaviour
{
    private const string baseUrl    = "https://dreamlo.com/lb/";
    private const string privateCode = "H8VkVT0AAUeKTxsISNMuGwOMDcgfYetUmM11mCRctqvg"; // Your private code (write access)
    private const string publicCode  = "6831a3828f40bb151441d238";                    // Your public  code (read  access)

    /// <summary>
    /// Upload a new score to the leaderboard.
    /// </summary>
    public IEnumerator UploadScore(string playerName, int score, int seconds)
    {
        string url = $"{baseUrl}{privateCode}/add/" +
                     $"{UnityWebRequest.EscapeURL(playerName)}/" +
                     $"{score}/{seconds}";

        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError($"[Dreamlo] Upload failed: {req.error}");
    }

    /// <summary>
    /// Download the first <paramref name="topN"/> entries and invoke <paramref name="onDone"/>.
    /// </summary>
    public IEnumerator DownloadTopScores(int topN, Action<List<Entry>> onDone)
    {
        string url = $"{baseUrl}{publicCode}/pipe/0/{topN}";
        using var req = UnityWebRequest.Get(url);

        Debug.Log($"[Dreamlo] Request: {url}");
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onDone?.Invoke(ParsePipe(req.downloadHandler.text));
        else
            Debug.LogError($"[Dreamlo] Download failed: {req.error}");
    }

    /// <summary>
    /// Parse Dreamlo “pipe” format: name|score|seconds||date|index
    /// </summary>
    private List<Entry> ParsePipe(string raw)
    {
        var list = new List<Entry>();

        foreach (string line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var p = line.Split('|');
            if (p.Length < 6) continue;                        // Skip malformed rows

            var e = new Entry
            {
                name    = p[0],
                score   = int.TryParse(p[1], out var sc)  ? sc  : 0,
                seconds = int.TryParse(p[2], out var sec) ? sec : 0,
                rank    = int.TryParse(p[5], out var idx) ? idx + 1 : 0   // Convert 0-based to 1-based
            };

            list.Add(e);
        }

        // Optional: resort by score if you do not trust the server order
        // list.Sort((a, b) => b.score.CompareTo(a.score));
        // for (int i = 0; i < list.Count; i++) list[i].rank = i + 1;

        return list;
    }
}

/// <summary>
/// Plain-old data object representing one leaderboard row.
/// </summary>
[Serializable]
public class Entry
{
    public int    rank;    // 1, 2, 3 …
    public string name;    // Player name
    public int    score;   // Points
    public int    seconds; // Time in seconds
}
