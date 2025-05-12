using System;
using UnityEngine;

[RequireComponent(typeof(S_SuperJump_Module))]
public class S_JumpEffect : MonoBehaviour
{
    [Header("Références VFX")]
    [Tooltip("Le prefab root (GameObject vide) contenant tous les children ParticleSystem")]
    [SerializeField] private GameObject jumpVFXRootPrefab;

    [Tooltip("Le parent (GameObject de la scène avec VFXRootController)")]
    [SerializeField] private Transform vfxRoot;

    [Header("Placement au sol")]
    [Tooltip("Layer(s) à considérer comme sol pour le raycast")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Distance max du raycast vers le bas (en unités Unity)")]
    [SerializeField] private float maxRayDistance = 10f;

    private S_SuperJump_Module jumpModule;

    private void Start()
    {
        jumpModule = GetComponent<S_SuperJump_Module>();
        jumpModule.OnJumpStateChange += OnJump;
    }

    private void OnDestroy()
    {
        if (jumpModule != null)
            jumpModule.OnJumpStateChange -= OnJump;
    }

    private void OnJump(Enum state, int level)
    {
        // On ne déclenche que sur l'état Jump et palier >=2
        if (state is PlayerStates.JumpState js && js == PlayerStates.JumpState.Jump && level > 1)
            SpawnJumpEffect();
    }

    private void SpawnJumpEffect()
    {
        if (jumpVFXRootPrefab == null || vfxRoot == null)
        {
            Debug.LogWarning("S_JumpEffect : référence manquante (prefab ou vfxRoot)", this);
            return;
        }

        // Origine du raycast légèrement au-dessus de la position du joueur pour éviter les self-colliders
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // Instanciation du VFX au point d'impact
            var go = Instantiate(jumpVFXRootPrefab, hit.point, Quaternion.identity, vfxRoot);

            // Démarrage de tous les ParticleSystems enfants
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                ps.Play();
            }
        }
        else
        {
            Debug.LogWarning($"S_JumpEffect : pas de sol détecté dans {maxRayDistance} unités", this);
        }
    }
}