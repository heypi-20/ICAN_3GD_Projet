using System;
using UnityEngine;

[RequireComponent(typeof(S_SuperJump_Module))]
public class S_JumpEffect : MonoBehaviour
{
    [Header("R�f�rences VFX")]
    [Tooltip("Le prefab root (GameObject vide) contenant tous les children ParticleSystem")]
    [SerializeField] private GameObject jumpVFXRootPrefab;
    [Tooltip("Le parent sous lequel on instanciera tous les VFX (avec VFXRootController)")]
    [SerializeField] private Transform vfxRoot;
    [Tooltip("Layer(s) � consid�rer comme sol pour le raycast")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distance max du raycast pour toucher le sol (en unit�s Unity)")]
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
        // On se d�clenche uniquement sur l'�v�nement "Jump"
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
            // Instanciation � l'emplacement du sol
            var go = Instantiate(jumpVFXRootPrefab, hit.point, Quaternion.identity, vfxRoot);

            // On d�marre tous les ParticleSystems enfants
            foreach (var ps in go.GetComponentsInChildren<ParticleSystem>())
                ps.Play();

            // Optionnel : d�truire l'instance au bout de X secondes 
            // (ajuste 5f selon la dur�e de ton effet ou g�re le StopAction sur chaque PS)
            Destroy(go, 5f);
        }
        else
        {
            Debug.LogWarning($"S_JumpEffect : pas de sol d�tect� sous le joueur dans {maxRayDistance} unit�s.", this);
        }
    }
}