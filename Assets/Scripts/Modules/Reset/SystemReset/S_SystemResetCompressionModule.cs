using System.Collections;
using UnityEngine;

public class S_SystemResetCompressionModule : MonoBehaviour
{
    public int systemResetCompressionTimes = 10; // Nombre de compressions par appel
    public bool random = false; // Utiliser un nombre al�atoire de compressions ?
    public Vector2 randomNumber = new Vector2(1, 10); // Intervalle pour le nombre al�atoire de compressions

    private bool isCompressing = false; // Emp�che le d�clenchement multiple de la compression

    private S_SystemResetModule systemResetModule;

    private void Start()
    {
        // Obtenir la r�f�rence au module de r�initialisation du syst�me
        systemResetModule = GetComponent<S_SystemResetModule>();
    }

    public void CompressReset()
    {
        // �viter d'appeler la compression si elle est d�j� en cours
        if (isCompressing) return;

        // D�terminer le nombre de compressions � effectuer
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
                systemResetModule.InvokeSystemeResetEvent(); // D�clencher l'�v�nement de r�initialisation du syst�me
            }
            yield return null; // Attendre une frame pour minimiser l'impact sur la performance
        }

        isCompressing = false;
    }
}
