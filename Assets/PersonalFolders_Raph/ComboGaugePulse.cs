using UnityEngine;
using DG.Tweening;

public class ComboGaugePulse : MonoBehaviour
{
    public Material comboMat;
    public string progressProperty = "_Progress";
    public float pulseScale = 1.1f;
    public float pulseDuration = 0.2f;
    public float idleAmplitude = 0.03f;
    public float idleSpeed = 1f;

    private Vector3 baseScale;
    private float idleTime = 0f;
    private bool hasPulsed = false;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float progress = comboMat.GetFloat(progressProperty);

        // Déclenche une impulsion uniquement à la frame où on arrive à 0
        if (progress <= 0.001f && !hasPulsed)
        {
            hasPulsed = true;

            transform.DOKill();
            transform.localScale = baseScale;
            transform.DOPunchScale(baseScale * (pulseScale - 1f), pulseDuration, 1, 0.5f);
        }
        else if (progress > 0.01f)
        {
            hasPulsed = false;
        }

        // Oscillation idle constante
        idleTime += Time.deltaTime * idleSpeed;
        float scaleOffset = 1f + Mathf.Sin(idleTime * Mathf.PI * 2f) * idleAmplitude;
        transform.localScale = baseScale * scaleOffset;
    }
}