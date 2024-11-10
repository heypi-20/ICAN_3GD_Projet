using System.Collections;
using UnityEngine;

public class S_SystemResetCompressionModule : MonoBehaviour
{
    public int systemResetCompressionTimes = 10; // Nombre de compressions par appel
    public bool random = false; // Utiliser un nombre aléatoire de compressions ?
    public Vector2 randomNumber = new Vector2(1, 10); // Intervalle pour le nombre aléatoire de compressions

    private bool isCompressing = false; // Empêche le déclenchement multiple de la compression

    private S_SystemResetModule systemResetModule;

    private void Start()
    {
        // Obtenir la référence au module de réinitialisation du système
        systemResetModule = GetComponent<S_SystemResetModule>();
    }

    public void CompressReset()
    {
        // Éviter d'appeler la compression si elle est déjà en cours
        if (isCompressing) return;

        // Déterminer le nombre de compressions à effectuer
        int compressionCount = random ? Random.Range((int)randomNumber.x, (int)randomNumber.y + 1) : systemResetCompressionTimes;

        // Commencer la compression
        StartCoroutine(CompressResetCoroutine(compressionCount));
    }

    private IEnumerator CompressResetCoroutine(int compressionCount)
    {
        isCompressing = true;

        for (int i = 0; i < compressionCount; i++)
        {
            if (systemResetModule != null)
            {
                systemResetModule.InvokeSystemeResetEvent(); // Déclencher l'événement de réinitialisation du système
            }
            yield return null; // Attendre une frame pour minimiser l'impact sur la performance
        }

        isCompressing = false;
    }
}
