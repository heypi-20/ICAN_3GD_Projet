using UnityEngine;
using TMPro;

public class S_CreateNewPlayerName : MonoBehaviour
{
    public TMP_InputField nameInputField;
    private const string playerNameKey = "PlayerName";  // 本地存储的 key

    void Start()
    {
        // 如果之前保存过名字，就自动填入
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            nameInputField.text = PlayerPrefs.GetString(playerNameKey);
        }

        // 添加事件监听：当玩家结束输入时保存（你也可以改为按钮点击时保存）
        nameInputField.onEndEdit.AddListener(SavePlayerName);
    }

    void SavePlayerName(string newName)
    {
        newName = newName.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            PlayerPrefs.SetString(playerNameKey, newName);
            PlayerPrefs.Save();  // 虽然一般自动保存，但加上更稳妥
            Debug.Log($"玩家名字已保存: {newName}");
        }
    }
}