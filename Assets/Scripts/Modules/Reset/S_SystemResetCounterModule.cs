using UnityEngine;
using TMPro;

[RequireComponent(typeof(S_SystemResetModule))]
public class S_SystemResetCounterModule : MonoBehaviour
{
    public TMP_Text resetCountText; // Référence optionnelle à un TMP_Text pour afficher le nombre de resets du système
    public int SystemResetCount; // Nombre de resets du système, accessible et modifiable publiquement

    private S_SystemResetModule systemResetModule;

    private void Start()
    {
        // Obtenir une référence au module de reset du système
        systemResetModule = GetComponent<S_SystemResetModule>();
        if (systemResetModule != null)
        {
            systemResetModule.SystemResetEvent += OnSystemReset;
        }

        // Initialiser le texte du compteur si la référence est définie
        UpdateResetCountText();
    }

    private void OnSystemReset()
    {
        // Incrémenter le compteur de resets du système et mettre à jour l'affichage
        SystemResetCount++;
        UpdateResetCountText();
    }

    private void UpdateResetCountText()
    {
        if (resetCountText != null)
        {
            resetCountText.text = "System Resets: " + SystemResetCount;
        }
    }
}
