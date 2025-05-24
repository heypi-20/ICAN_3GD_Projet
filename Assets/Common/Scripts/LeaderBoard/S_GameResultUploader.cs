using System.Collections;
using UnityEngine;
using TMPro;                 // 用的是 TMP_InputField

public class S_GameResultUploader : MonoBehaviour
{
    [Header("引用组件")]
    public S_DreamloManager dreamlo;          // 场景里放一个 DreamloManager，再拖进来
    public S_GameResultCalcul resultCalc;     // 你的计分脚本
    public TMP_InputField nameInputField;     // 玩家在 UI 里输入名字的 InputField
    

    public void OnClickNext()
    {
            StartCoroutine(UploadCurrentResult());
    }

    IEnumerator UploadCurrentResult()
    {
        // 1. 拿到玩家名字
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        // 2. 从计分脚本拿总分 & 时长
        int   finalScore   = resultCalc.finalScore;  // 你 ShowResultScreen 里已经写进去
        float totalSeconds = resultCalc.timeChrono;       // 或者 resultCalc 里算好的

        // 3. 调 Dreamlo 的协程
        yield return StartCoroutine(
            dreamlo.UploadScore(playerName,
                finalScore,
                Mathf.RoundToInt(totalSeconds))
        );

        Debug.Log($"[Dreamlo] 上传完成：{playerName} - {finalScore} 分");
    }
}