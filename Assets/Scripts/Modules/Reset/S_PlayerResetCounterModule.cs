using UnityEngine;
using TMPro;

[RequireComponent(typeof(S_PlayerResetModule))]
public class S_PlayerResetCounterModule : MonoBehaviour
{
    public TMP_Text resetCountText; // Référence optionnelle à un TMP_Text pour afficher le nombre de resets
    public int PlayerResetCount; // Nombre de resets du joueur, accessible et modifiable publiquement

    private S_PlayerResetModule playerResetModule;

    private void Start()
    {
        // Obtenir une référence au module de reset du joueur
        playerResetModule = GetComponent<S_PlayerResetModule>();
        if (playerResetModule != null)
        {
            playerResetModule.PlayerResetEvent += OnPlayerReset;
        }

        // Initialiser le texte du compteur si la référence est définie
        UpdateResetCountText();
    }

    private void OnPlayerReset()
    {
        // Incrémenter le compteur de resets et mettre à jour l'affichage
        PlayerResetCount++;
        UpdateResetCountText();
    }

    private void UpdateResetCountText()
    {
        if (resetCountText != null)
        {
            resetCountText.text = "Player Resets: " + PlayerResetCount;
        }
    }
}
