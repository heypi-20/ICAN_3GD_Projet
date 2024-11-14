using UnityEngine;

public class S_PlayerTeleportModule : MonoBehaviour
{
    public GameObject player; // Joueur � t�l�porter
    public int requiredResetCount = 3; // Nombre d'appels n�cessaires avant la t�l�portation
    public Transform teleportLocation; // Position de t�l�portation cible

    private int currentCallCount = 0; // Compteur des appels de la m�thode



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IncrementCallCountAndTeleport();
        }
    }
    // M�thode � appeler pour incr�menter le compteur et potentiellement t�l�porter le joueur
    public void IncrementCallCountAndTeleport()
    {
        currentCallCount++;

        if (currentCallCount >= requiredResetCount)
        {
            TeleportPlayer();
            currentCallCount = 0; // R�initialiser le compteur apr�s la t�l�portation
        }
    }

    // T�l�porter le joueur � l'emplacement d�fini
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
