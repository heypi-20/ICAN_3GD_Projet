using System;
using UnityEngine;

[RequireComponent(typeof(S_SuperJump_Module))]
public class S_JumpEffect : MonoBehaviour
{
    [Header("Références VFX")]
    [Tooltip("Le prefab root (GameObject vide) contenant tous les children ParticleSystem")]
    [SerializeField] private GameObject jumpVFXRootPrefab;
    [Tooltip("Le parent sous lequel on instanciera tous les VFX (avec VFXRootController)")]
    [SerializeField] private Transform vfxRoot;
    [Tooltip("Layer(s) à considérer comme sol pour le raycast")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distance max du raycast pour toucher le sol (en unités Unity)")]
    [SerializeField] private float maxRayDistance = 5f;

    private S_SuperJump_Module jumpModule;

    private void Start()
    {
        jumpModule = GetComponent<S_SuperJump_Module>();
        jumpModule.OnJumpStateChange += HandleJumpStateChanged;
    }

    private void OnDestroy()
    {
        if (jumpModule != null)
            jumpModule.OnJumpStateChange -= HandleJumpStateChanged;
    }

    private void HandleJumpStateChanged(Enum state, int level)
    {
        // On se déclenche uniquement sur l'événement "Jump"
        if (state is PlayerStates.JumpState js && js == PlayerStates.JumpState.Jump)
        {
            // Seulement si on est au palier >=2 (level 2/3/4)
            if (level > 1)
                SpawnJumpEffect();
        }
    }

    private void SpawnJumpEffect()
    {
        // On tire un rayon vers le bas depuis le joueur
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxRayDistance, groundLayer))
        {
            // Instanciation à l'emplacement du sol
            var go = Instantiate(jumpVFXRootPrefab, hit.point, Quaternion.identity, vfxRoot);

            // On démarre tous les ParticleSystems enfants
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            // Optionnel : détruire l'instance au bout de X secondes 
            // (ajuste 5f selon la durée de ton effet ou gère le StopAction sur chaque PS)
            Destroy(go, 5f);
        }
        else
        {
            Debug.LogWarning($"S_JumpEffect : pas de sol détecté sous le joueur dans {maxRayDistance} unités.", this);
        }
    }
}