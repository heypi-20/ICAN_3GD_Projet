using System;
using UnityEngine;

[RequireComponent(typeof(S_SuperJump_Module))]
public class S_JumpEffect : MonoBehaviour
{
    [Header("Références VFX")]
    [Tooltip("Le prefab ParticleSystem à jouer lors du jump")]
    [SerializeField] private ParticleSystem jumpVFXPrefab;
    [Tooltip("Le VFX Root (GameObject sur lequel est attaché VFXRootController)")]
    [SerializeField] private Transform vfxRoot;
    [Tooltip("LayerMask du sol pour détecter où faire apparaître l'effet")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Hauteur max de la recherche du sol")]
    [SerializeField] private float maxRayDistance = 5f;

    private S_SuperJump_Module jumpModule;

    private void Start()
    {
        jumpModule = GetComponent<S_SuperJump_Module>();
        S_PlayerStateObserver.Instance.OnJumpStateEvent += HandleJumpStateChanged;
    }



    private void HandleJumpStateChanged(Enum jumpState, int level)
    {
        // Vérifier que c'est bien l'événement de saut
        if (jumpState is PlayerStates.JumpState js && js == PlayerStates.JumpState.Jump)
        {
            SpawnJumpEffect();
        }
    }

    private void SpawnJumpEffect()
    {
        // Lancer un rayon vers le bas pour trouver la position du sol
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxRayDistance, groundLayer))
        {
            // Instancier le VFX au point d'impact, sans rotation particulière
            var instance = Instantiate(jumpVFXPrefab, hit.point, Quaternion.identity, vfxRoot);
            instance.Play();
        }
        else
        {
            Debug.LogWarning("Sol non détecté sous le joueur pour le JumpEffect.", this);
        }
    }
}