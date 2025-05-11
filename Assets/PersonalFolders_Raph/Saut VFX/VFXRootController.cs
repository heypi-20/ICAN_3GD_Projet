using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class VFXRootController : MonoBehaviour
{
    [Header("Param�tres globaux pour tous les PS enfants")]
    public bool loop = false;
    public bool playOnAwake = false;

    private List<ParticleSystem> childSystems = new List<ParticleSystem>();

    void OnValidate()
    {
        // En �diteur, recalcule la liste et applique
        RefreshChildList();
        ApplyLoopAwake();
    }

    void Awake()
    {
        // Au d�marrage, recalcule et applique
        RefreshChildList();
        ApplyLoopAwake();
    }

    /// <summary>
    /// R�cup�re tous les ParticleSystem sous ce GameObject (root + enfants)
    /// </summary>
    void RefreshChildList()
    {
        childSystems.Clear();
        childSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
    }

    /// <summary>
    /// Pour chaque PS enfant, copie loop & playOnAwake depuis le root
    /// </summary>
    void ApplyLoopAwake()
    {
        foreach (var ps in childSystems)
        {
            var main = ps.main;
            main.loop = loop;
            main.playOnAwake = playOnAwake;

            // Si on veut lancer automatiquement, on play immediately
            if (playOnAwake && !ps.isPlaying)
                ps.Play();
        }
    }
}