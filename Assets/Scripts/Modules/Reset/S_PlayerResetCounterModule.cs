using UnityEngine;
using TMPro;

[RequireComponent(typeof(S_PlayerResetModule))]
public class S_PlayerResetCounterModule : MonoBehaviour
{
    public TMP_Text resetCountText; // R�f�rence optionnelle � un TMP_Text pour afficher le nombre de resets
    public int PlayerResetCount; // Nombre de resets du joueur, accessible et modifiable publiquement

    private S_PlayerResetModule playerResetModule;

    private void Start()
    {
        // Obtenir une r�f�rence au module de reset du joueur
        playerResetModule = GetComponent<S_PlayerResetModule>();
        if (playerResetModule != null)
        {
            playerResetModule.PlayerResetEvent += OnPlayerReset;
        }

        // Initialiser le texte du compteur si la r�f�rence est d�finie
        UpdateResetCountText();
    }

    private void OnPlayerReset()
    {
        // Incr�menter le compteur de resets et mettre � jour l'affichage
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
