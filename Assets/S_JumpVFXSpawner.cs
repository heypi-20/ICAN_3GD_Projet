using UnityEngine;
using System.Linq;               // pour utiliser Max() ou FirstOrDefault()
using System;

[RequireComponent(typeof(S_SuperJump_Module))]
public class JumpVFXSpawner : MonoBehaviour
{
    [Header("Glisse ici ton instance unique, désactivée")]
    public S_ParentJumpVFX jumpVFXInstance;

    private S_SuperJump_Module jumpModule;
    private ParticleSystem[] childPS;

    void Awake()
    {
        jumpModule = GetComponent<S_SuperJump_Module>();

        // Abonnement à l'évènement (OnJumpStateChange)
        // On utilise un lambda pour simplifier le binding
        jumpModule.OnJumpStateChange += (state, level) =>
        {
            // Quand on appuie sur Jump (pas OnAir/OnGround), et palier ≥ 2
            if (state.Equals(PlayerStates.JumpState.Jump) && level >= 2)
                PlayVFX(level);
        };

        // Si tu n’as pas glissé l’instance dans l’inspector, on la cherche une fois
        if (jumpVFXInstance == null)
            jumpVFXInstance = FindObjectOfType<S_ParentJumpVFX>();

        // Récupère tous les PS enfants pour pouvoir Play/Stop
        childPS = jumpVFXInstance.GetComponentsInChildren<ParticleSystem>();

        // On garde l’instance désactivée jusqu’au premier affichage
        jumpVFXInstance.gameObject.SetActive(false);
    }

    private void PlayVFX(int level)
    {
        // 1) On récupère les paramètres du palier
        var jl = jumpModule.jumpLevels.FirstOrDefault(j => j.level == level);
        if (jl == null) return;

        // 2) On positionne au sol sous le joueur
        Vector3 pos = transform.position;
        if (Physics.Raycast(pos, Vector3.down, out var hit, 100f))
            pos = hit.point;
        jumpVFXInstance.transform.position = pos;

        // 3) On scale à 50% du vortex
        jumpVFXInstance.globalScale = jl.VortexRange * 0.5f;
        jumpVFXInstance.ApplyAllSettings();

        // 4) On active + on joue tous les PS
        jumpVFXInstance.gameObject.SetActive(true);
        foreach (var ps in childPS)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        // 5) On recache après la durée max (duration + lifetime)
        float maxDur = childPS.Max(ps => ps.main.duration + ps.main.startLifetime.constantMax);
        Invoke(nameof(HideVFX), maxDur);
    }

    private void HideVFX()
    {
        jumpVFXInstance.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // Désabonnement propre
        jumpModule.OnJumpStateChange -= null;
    }
}