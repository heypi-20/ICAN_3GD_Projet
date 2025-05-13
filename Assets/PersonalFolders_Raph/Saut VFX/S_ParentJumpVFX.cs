using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class S_ParentJumpVFX : MonoBehaviour
{
    [Header("Paramètres globaux pour tous les PS enfants")]
    [Tooltip("Scale uniforme de tout l'effet (transform du parent)")]
    [Range(1f, 60f)]
    public float globalScale = 1f;

    [Tooltip("Les PS loopent-ils ?")]
    public bool loop = false;
    [Tooltip("Les PS démarrent-ils en Awake ?")]
    public bool playOnAwake = false;

    private List<ParticleSystem> childSystems = new List<ParticleSystem>();

    // Appelé à chaque changement dans l'inspecteur
    void OnValidate()
    {
        ApplyAllSettings();
    }

    // Appelé au runtime, juste avant le premier Update
    void Awake()
    {
        ApplyAllSettings();
    }

    private void ApplyAllSettings()
    {
        // 1) Scale uniforme du parent
        transform.localScale = Vector3.one * globalScale;

        // 2) Récupère et configure tous les PS enfants
        RefreshChildList();
        ApplyLoopAwake();
    }

    private void RefreshChildList()
    {
        childSystems.Clear();
        childSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
    }

    private void ApplyLoopAwake()
    {
        foreach (var ps in childSystems)
        {
            var main = ps.main;
            main.loop = loop;
            main.playOnAwake = playOnAwake;

            if (playOnAwake && !ps.isPlaying)
                ps.Play();
        }
    }
}