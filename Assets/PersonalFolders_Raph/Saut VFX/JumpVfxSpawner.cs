using System;
using UnityEngine;

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
            // Calcul de l'échelle en fonction du VortexRange du palier
            var jl = _superJump.jumpLevels.Find(j => j.level == level);
            float vortexRange = jl != null ? jl.VortexRange : 1f;
            float scale = vortexRange * (sizePercent / 100f);

            // Instanciation du prefab
            Vector3 spawnPos = hit.point + Vector3.up * 0.1f;
            GameObject vfx = Instantiate(vortexVFXPrefab, spawnPos, Quaternion.identity);

            // Applique la taille via ton script parent S_ParentJumpVFX
            var parentVfx = vfx.GetComponent<S_ParentJumpVFX>();
            if (parentVfx != null)
            {
                parentVfx.SetGlobalScale(scale);
            }
            else
            {
                // Fallback si jamais S_ParentJumpVFX est manquant
                vfx.transform.localScale = Vector3.one * scale;
            }

            // Démarrage manuel des ParticleSystems si nécessaire
            foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
                if (!ps.isPlaying)
                    ps.Play();

            // Stop Action: Destroy devrait se charger de la destruction automatique
        }
    }
}