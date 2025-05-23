using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserMark : MonoBehaviour
{
    public TMP_Text RankText;
    public TMP_Text NameText;
    public TMP_Text TimeText;
    public TMP_Text ScoreText;

    public void SetValues(int rank, string name, float time, int score)
    {
        RankText.text = rank.ToString();
        NameText.text = name;
        TimeText.text = FormatTime(time);
        ScoreText.text = score.ToString();
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int milliseconds = (int)((time * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}:{milliseconds:000}";
    }
}
