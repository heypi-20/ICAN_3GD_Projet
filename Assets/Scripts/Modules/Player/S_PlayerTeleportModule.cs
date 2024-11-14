using UnityEngine;

public class S_PlayerTeleportModule : MonoBehaviour
{
    public GameObject player; // Joueur à téléporter
    public int requiredResetCount = 3; // Nombre d'appels nécessaires avant la téléportation
    public Transform teleportLocation; // Position de téléportation cible

    private int currentCallCount = 0; // Compteur des appels de la méthode



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IncrementCallCountAndTeleport();
        }
    }
    // Méthode à appeler pour incrémenter le compteur et potentiellement téléporter le joueur
    public void IncrementCallCountAndTeleport()
    {
        currentCallCount++;

        if (currentCallCount >= requiredResetCount)
        {
            TeleportPlayer();
            currentCallCount = 0; // Réinitialiser le compteur après la téléportation
        }
    }

    // Téléporter le joueur à l'emplacement défini
    private void TeleportPlayer()
    {
        if (player != null)
        {
            player.transform.position = teleportLocation.position;
            player.transform.rotation = teleportLocation.rotation;
            Debug.Log("Player has been teleported to: " + teleportLocation);
        }
        else
        {
            Debug.LogWarning("Player reference is not assigned.");
        }
    }
}
