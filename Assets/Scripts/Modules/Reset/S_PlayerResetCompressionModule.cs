using System.Collections;
using UnityEngine;

public class S_PlayerResetCompressionModule : MonoBehaviour
{
    public int playerResetCompressionTimes = 10; // Nombre de compressions par appel
    public bool random = false; // Utiliser un nombre aléatoire de compressions ?
    public Vector2 randomNumber = new Vector2(1, 10); // Intervalle pour le nombre aléatoire de compressions

    private bool isCompressing = false; // Empêche le déclenchement multiple de la compression

    private S_PlayerResetModule playerResetModule;

    private void Start()
    {
        // Obtenir la référence au module de réinitialisation du joueur
        playerResetModule = GetComponent<S_PlayerResetModule>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CompressReset();
        }
    }

    public void CompressReset()
    {
        // Éviter d'appeler la compression si elle est déjà en cours
        if (isCompressing) return;

        // Déterminer le nombre de compressions à effectuer
        int compressionCount = random ? Random.Range((int)randomNumber.x, (int)randomNumber.y + 1) : playerResetCompressionTimes;

        // Commencer la compression
        StartCoroutine(CompressResetCoroutine(compressionCount));
    }

    private IEnumerator CompressResetCoroutine(int compressionCount)
    {
        isCompressing = true;

        for (int i = 0; i < compressionCount; i++)
        {
            if (playerResetModule != null)
            {
                playerResetModule.InvokePlayerResetEvent(); // Déclencher l'événement de réinitialisation du joueur
            }
            yield return null; // Attendre une frame pour minimiser l'impact sur la performance
        }

        isCompressing = false;
    }
}
