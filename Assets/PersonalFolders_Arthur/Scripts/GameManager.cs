using UnityEngine;
using UnityEngine.UI; // Nécessaire pour utiliser Text
using TMPro; // Nécessaire pour utiliser TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager instance; // Singleton pour un accès global
    public int energyPoints = 0; // Variable pour suivre les points d'énergie

    public int EP_Cross = 3;
    public int EP_AddShoot = 3;
    public int EP_ReduceShoot = 1;
    public int EP_UsedJump_Noeud = 5;

    [Header("Références UI")]
    public Text energyText; // Pour un UI Text classique
    public TextMeshProUGUI energyTMPText; // Pour TextMeshPro

    private void Awake()
    {
        // Configure le singleton
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space)))
        {
            if (energyPoints <= 0) return;
            UsedNoeud_Jump();
        }

    }

    public void AddEnergyPointOnDestroy()
    {
        energyPoints = energyPoints + EP_AddShoot; // Incrémente les points d'énergie
        UpdateEnergyUI(); // Met à jour l'UI
        Debug.Log("Points d'énergie : " + energyPoints);
    }
    public void AddEnergyPointOnCross()
    {
        energyPoints = energyPoints + EP_Cross; // Incrémente les points d'énergie
        UpdateEnergyUI(); // Met à jour l'UI
        Debug.Log("Points d'énergie : " + energyPoints);
    }
    public void UsedNoeud_Jump()
    {
        energyPoints = energyPoints - EP_Cross; // Incrémente les points d'énergie
        UpdateEnergyUI(); // Met à jour l'UI
        Debug.Log("Points d'énergie : " + energyPoints);
    }
    public void AddEnergyPointOnShoot()
    {
        energyPoints = energyPoints - EP_ReduceShoot; // Incrémente les points d'énergie
        UpdateEnergyUI(); // Met à jour l'UI
        Debug.Log("Points d'énergie : " + energyPoints);
    }
    

    private void UpdateEnergyUI()
    {
        // Mets à jour le texte en fonction du type utilisé
        if (energyText != null)
        {
            energyText.text = "Energie: " + energyPoints;
        }

        if (energyTMPText != null)
        {
            energyTMPText.text = "Energie: " + energyPoints;
        }
    }
}