using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class S_DreamloManager : MonoBehaviour
{
    const string baseUrl = "https://dreamlo.com/lb/";
    private string privateCode="H8VkVT0AAUeKTxsISNMuGwOMDcgfYetUmM11mCRctqvg"; // Inspector 里贴上
    private string publicCode="6831a3828f40bb151441d238"; // Inspector 里贴上
    
    public IEnumerator UploadScore(string playerName, int score, int seconds)
    {
        string url = $"{baseUrl}{privateCode}/add/" + // 固定片段
                     $"{UnityWebRequest.EscapeURL(playerName)}/" + // 名字要转义
                     $"{score}/{seconds}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("Dreamlo Upload Error: " + req.error);
    }

    
    public IEnumerator DownloadTopScores(int topN, Action<List<Entry>> onDone)
    {
        string url = $"{baseUrl}{publicCode}/pipe/0/{topN}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onDone?.Invoke(ParsePipe(req.downloadHandler.text));
        else
            Debug.LogError("Dreamlo Download Error: " + req.error);
    }

    List<Entry> ParsePipe(string raw)
    {
        var list = new List<Entry>();
        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            // 0:rank 1:name 2:score 3:seconds 4:date 5:text
            var p = line.Split('|');
            if (p.Length < 6) continue;

            list.Add(new Entry
            {
                rank = int.Parse(p[0]),
                name = p[1],
                score = int.Parse(p[2]),
                seconds = int.Parse(p[3]), // 90
            });
        }

        return list;
    }
}
    [Serializable]
    public class Entry
    {
        public int rank;
        public string name;
        public int score; // points
        public int seconds; // raw seconds
    }

