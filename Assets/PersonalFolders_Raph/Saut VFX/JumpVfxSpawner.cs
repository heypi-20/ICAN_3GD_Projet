using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(S_SuperJump_Module))]
public class JumpVfxSpawner : MonoBehaviour
{
    [Header("Vortex VFX Prefab")]
    [SerializeField] private GameObject vortexVFXPrefab;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 90f;

    [Header("Size Settings")]
    [Tooltip("Taille du VFX en % du VortexRange du palier")]
    [Range(0f, 100f)]
    [SerializeField] private float sizePercent = 100f;

    private S_SuperJump_Module _superJump;

    private void Awake()
    {
        _superJump = GetComponent<S_SuperJump_Module>();
        _superJump.OnJumpStateChange += OnJumpStateChange;
    }

    private void OnDestroy()
    {
        _superJump.OnJumpStateChange -= OnJumpStateChange;
    }

    private void OnJumpStateChange(Enum stateEnum, int level)
    {
        // On ne s'intéresse qu'au moment du saut
        if (!(stateEnum is PlayerStates.JumpState state) || state != PlayerStates.JumpState.Jump)
            return;
        if (level < 2 || level > 4)
            return;

        // Détection du sol
        if (Physics.Raycast(transform.position, Vector3.down,
                            out RaycastHit hit, raycastDistance, groundLayer))
        {
            // 1) On calcule la taille cible
            // Récupère le JumpLevel correspondant pour accéder à VortexRange
            var jumpLevels = _superJump.jumpLevels;
            var jl = jumpLevels.Find(j => j.level == level);
            float vortexRange = (jl != null) ? jl.VortexRange : 1f;
            float scale = vortexRange * (sizePercent / 100f);

            // 2) Instanciation
            Vector3 spawnPos = hit.point + Vector3.up * 0.1f;
            GameObject vfx = Instantiate(vortexVFXPrefab, spawnPos, Quaternion.identity);

            // 3) Application de l’échelle
            vfx.transform.localScale = Vector3.one * scale;

            // 4) Lancement manuel des ParticleSystems (si besoin)
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            // -> Pas de Destroy ici : "Stop Action: Destroy" sur le prefab s'en chargera.
        }
    }
}