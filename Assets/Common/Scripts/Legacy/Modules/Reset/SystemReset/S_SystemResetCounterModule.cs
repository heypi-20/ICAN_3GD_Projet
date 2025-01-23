using UnityEngine;
using TMPro;

[RequireComponent(typeof(S_SystemResetModule))]
public class S_SystemResetCounterModule : MonoBehaviour
{
    public TMP_Text resetCountText; // R�f�rence optionnelle � un TMP_Text pour afficher le nombre de resets du syst�me
    public int SystemResetCount; // Nombre de resets du syst�me, accessible et modifiable publiquement

    private S_SystemResetModule systemResetModule;

    private void Start()
    {
        // Obtenir une r�f�rence au module de reset du syst�me
        systemResetModule = GetComponent<S_SystemResetModule>();
        if (systemResetModule != null)
        {
            systemResetModule.SystemResetEvent += OnSystemReset;
        }

        // Initialiser le texte du compteur si la r�f�rence est d�finie
        UpdateResetCountText();
    }

    private void OnSystemReset()
    {
        // Incr�menter le compteur de resets du syst�me et mettre � jour l'affichage
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
