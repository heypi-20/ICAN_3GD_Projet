using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class S_ParticuleEffectJump : MonoBehaviour
{
    [Header("Référence")]
    public ParticleSystem ps;

    [Header("Paramètres")]
    public bool loop = false;
    public bool playOnAwake = false;
    public float duration = 2.5f;
    public float startLifetime = 2.5f;

    void OnValidate()
    {
        if (!ps)
            ps = GetComponent<ParticleSystem>();

        ApplyParameters();
    }

    void Start()
    {
        ApplyParameters();

        if (playOnAwake)
            ps.Play();
    }

    [ContextMenu("Appliquer les paramètres")]
    public void ApplyParameters()
    {
        // ⚠️ Important : Arrêter le PS avant de changer duration
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear();

        var main = ps.main;
        main.loop = loop;
        main.playOnAwake = playOnAwake;
        main.duration = duration;
        main.startLifetime = startLifetime;
    }

    public void PlayEffect()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear();
        ps.Play();
    }

    public void StopEffect()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}