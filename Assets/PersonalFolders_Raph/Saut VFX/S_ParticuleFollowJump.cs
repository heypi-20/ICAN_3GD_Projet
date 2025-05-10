using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class S_particuleFollowJump : MonoBehaviour
{
    public ParticleSystem PsParticule;

    private ParticleSystem parentPs;
    private ParticleSystem.MainModule parentMain;
    private ParticleSystem.ShapeModule parentShape;
    private ParticleSystem.SizeOverLifetimeModule parentSOL;
    private AnimationCurve parentCurve;
    private float baseStartSize;
    private float plateauEnd;

    // Modules enfant
    private ParticleSystem.MainModule childMain;
    private ParticleSystem.ShapeModule childShape;
    private ParticleSystem.EmissionModule childEmission;

    void Awake()
    {
        // 1) Récupère le PS parent et ses modules
        parentPs = GetComponent<ParticleSystem>();
        parentMain = parentPs.main;
        parentShape = parentPs.shape;
        parentSOL = parentPs.sizeOverLifetime;
        parentCurve = parentSOL.size.curve;
        baseStartSize = parentMain.startSize.constant;

        // 2) Calcule tNorm max où la courbe est à son plateau
        float maxVal = float.MinValue;
        foreach (var key in parentCurve.keys)
            maxVal = Mathf.Max(maxVal, key.value);
        plateauEnd = 0f;
        foreach (var key in parentCurve.keys)
            if (Mathf.Approximately(key.value, maxVal))
                plateauEnd = Mathf.Max(plateauEnd, key.time);

        // 3) Récupère le PS enfant et ses modules
        if (PsParticule == null)
        {
            Debug.LogError("CircleAndChildPSController: il faut assigner childPs dans l'inspector !");
            enabled = false;
            return;
        }

        childMain = PsParticule.main;
        childShape = PsParticule.shape;
        childEmission = PsParticule.emission;

        // 4) Synchronise loop & playOnAwake
        childMain.loop = parentMain.loop;
        childMain.playOnAwake = parentMain.playOnAwake;

        // 5) Si le parent démarre à l’Awake, lance aussi l’enfant
        if (childMain.playOnAwake)
            PsParticule.Play();
    }

    void Update()
    {
        // 6) Si le parent n'est pas en train de jouer et ne loop pas → on coupe tout
        if (!parentPs.isPlaying && !parentMain.loop)
        {
            childEmission.enabled = false;
            PsParticule.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return;
        }

        // 7) Calcul du temps normalisé tNorm en gérant le loop
        float duration = parentMain.duration;
        float time = parentPs.time;
        float tNorm = parentMain.loop
            ? Mathf.Repeat(time, duration) / duration
            : time / duration;
        tNorm = Mathf.Clamp01(tNorm);

        // 8) Évalue la courbe et convertit en world radius
        float f = parentCurve.Evaluate(tNorm);
        float worldScale = parentPs.transform.lossyScale.x;
        float radius = (baseStartSize * 0.5f) * f * worldScale;
        childShape.radius = radius;

        // 9) Émission tant que tNorm <= plateauEnd
        bool emit = tNorm <= plateauEnd;
        if (emit)
        {
            if (!PsParticule.isPlaying)
                PsParticule.Play();
            childEmission.enabled = true;
        }
        else
        {
            childEmission.enabled = false;
            if (PsParticule.isPlaying)
                PsParticule.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    [ContextMenu("Resynchroniser Loop/Awake")]
    void SyncLoopAwake()
    {
        childMain.loop = parentMain.loop;
        childMain.playOnAwake = parentMain.playOnAwake;
    }
}