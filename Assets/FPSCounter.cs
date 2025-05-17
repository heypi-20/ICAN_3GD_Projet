using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;     // Lien vers le texte TMP
    public float refreshRate = 0.5f;    // Intervalle entre les mises à jour (en secondes)
    private float timer = 0f;

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        if (timer >= refreshRate)
        {
            float fps = 1.0f / Time.unscaledDeltaTime;
            fpsText.text = Mathf.Ceil(fps).ToString() + " FPS";
            timer = 0f;
        }
    }
}

