using UnityEngine;

public class S_DisplayGainScore : MonoBehaviour
{
    private S_ScorePopupAnimation s_ScorePopupAnimation;

    private void Start()
    {
        s_ScorePopupAnimation = gameObject.GetComponent<S_ScorePopupAnimation>();
    }
    public void ShowGainScore(float gainScore)
    {
        int displayScore = Mathf.CeilToInt(gainScore);
        s_ScorePopupAnimation.Show(displayScore);
    }
}