using UnityEngine;
using TMPro;

public class S_CreateNewPlayerName : MonoBehaviour
{
    public TMP_InputField nameInputField;
    private const string playerNameKey = "PlayerName";  // Key used for local storage

    void Start()
    {
        // If a name has been saved before, automatically populate the input field
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            nameInputField.text = PlayerPrefs.GetString(playerNameKey);
        }

        // Add a listener: save the name when the player finishes editing
        nameInputField.onEndEdit.AddListener(SavePlayerName);
    }

    void SavePlayerName(string newName)
    {
        newName = newName.Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            PlayerPrefs.SetString(playerNameKey, newName);
            PlayerPrefs.Save();  // Manually force save to ensure persistence
        }
    }
}