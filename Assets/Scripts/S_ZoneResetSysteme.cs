using System;
using UnityEngine;
using TMPro;  

public class S_ZoneResetSysteme : MonoBehaviour
{
    
    public static event Action OnZoneReset;// Définir un événement pour que d'autres objets puissent s'y abonner   
    public TextMeshProUGUI resetCountText;// Composant TextMeshPro UI pour afficher le nombre de réinitialisations
    [SerializeField] private KeyCode resetKey = KeyCode.R;// Touche configurable pour déclencher la réinitialisation de la zone

    private int resetCount = 0; // Compteur du nombre de réinitialisations déclenchées

    void Update()
    {
        // Lorsque la touche définie est pressée, déclencher l'événement de réinitialisation
        if (Input.GetKeyDown(resetKey))
        {
            TriggerZoneReset();
        }
    }

    // Méthode pour déclencher l'événement de réinitialisation
    private void TriggerZoneReset()
    {
        Debug.Log("Réinitialisation de la zone déclenchée!");

        // Incrémenter le compteur de réinitialisations
        resetCount++;

        // Mettre à jour l'affichage du texte dans l'UI
        UpdateResetCountText();

        // Déclencher l'événement s'il y a des abonnés
        OnZoneReset?.Invoke();
    }

    // Méthode pour mettre à jour le texte TextMeshPro
    private void UpdateResetCountText()
    {
        if (resetCountText != null)
        {
            resetCountText.text = $"Nombre de réinitialisations : {resetCount}";
        }
        else
        {
            Debug.LogWarning("Le composant TextMeshProUGUI n'est pas assigné.");
        }
    }
}
