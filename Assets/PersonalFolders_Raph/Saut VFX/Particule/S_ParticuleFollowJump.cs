using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class S_particuleFollowJump : MonoBehaviour
{
    [Header("Référence au PS parent (cercle)")]
    public ParticleSystem parentPs;

    // Modules du parent
    private ParticleSystem.MainModule parentMain;
    private ParticleSystem.ShapeModule parentShape;
    private ParticleSystem.SizeOverLifetimeModule parentSOL;
    private AnimationCurve parentCurve;
    private float baseStartSize;
    private float plateauEnd;

    // Modules de l’enfant
    private ParticleSystem childPs;
    private ParticleSystem.ShapeModule childShape;
    private ParticleSystem.EmissionModule childEmission;

    void Awake()
    {
        // 1) Récupère les modules du parent
        parentMain = parentPs.main;
        parentShape = parentPs.shape;
        parentSOL = parentPs.sizeOverLifetime;
        parentCurve = parentSOL.size.curve;
        baseStartSize = parentMain.startSize.constant;

        // 2) Calcule le tNorm max (fin de croissance / plateau)
        float maxVal = float.MinValue;
        foreach (var key in parentCurve.keys)
            maxVal = Mathf.Max(maxVal, key.value);
        plateauEnd = 0f;
        foreach (var key in parentCurve.keys)
            if (Mathf.Approximately(key.value, maxVal))
                plateauEnd = Mathf.Max(plateauEnd, key.time);

        // 3) Récupère l’enfant et ses modules
        childPs = GetComponent<ParticleSystem>();
        childShape = childPs.shape;
        childEmission = childPs.emission;
    }

    void Update()
    {
        // Si le parent est stoppé et ne boucle pas, on coupe l’émission
        if (!parentPs.isPlaying && !parentMain.loop)
        {
            childEmission.enabled = false;
            return;
        }

        // 4) Calcul du temps normalisé tNorm
        float dur = parentMain.duration;
        float time = parentPs.time;
        float tNorm = parentMain.loop
            ? Mathf.Repeat(time, dur) / dur
            : time / dur;
        tNorm = Mathf.Clamp01(tNorm);

        // 5) Évalue la courbe Size-over-Lifetime du parent
        float f = parentCurve.Evaluate(tNorm);
        // Convertit en rayon monde pour le Shape.radius
        float worldScale = parentPs.transform.lossyScale.x;
        float radius = (baseStartSize * 0.5f) * f * worldScale;
        childShape.radius = radius;

        // 6) Active l’émission pendant la phase de croissance + plateau
        bool emit = tNorm <= plateauEnd;
        childEmission.enabled = emit;
    }
}