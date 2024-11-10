using System.Collections;
using UnityEngine;

public class S_PlayerResetCompressionModule : MonoBehaviour
{
    public int playerResetCompressionTimes = 10; // Nombre de compressions par appel
    public bool random = false; // Utiliser un nombre al�atoire de compressions ?
    public Vector2 randomNumber = new Vector2(1, 10); // Intervalle pour le nombre al�atoire de compressions

    private bool isCompressing = false; // Emp�che le d�clenchement multiple de la compression

    private S_PlayerResetModule playerResetModule;

    private void Start()
    {
        // Obtenir la r�f�rence au module de r�initialisation du joueur
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
        // �viter d'appeler la compression si elle est d�j� en cours
        if (isCompressing) return;

        // D�terminer le nombre de compressions � effectuer
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
                playerResetModule.InvokePlayerResetEvent(); // D�clencher l'�v�nement de r�initialisation du joueur
            }
            yield return null; // Attendre une frame pour minimiser l'impact sur la performance
        }

        isCompressing = false;
    }
}
