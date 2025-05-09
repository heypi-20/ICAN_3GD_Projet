using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;

[RequireComponent(typeof(VisualEffect))]
public class S_SprintEffect : MonoBehaviour
{
    [Header("VFX Settings")]
    private VisualEffect vfx;
    private float initialSpawnRate;
    private float initialSpeed;

    [Header("Camera Settings")]
    public CinemachineVirtualCamera cinemachineCamera;
    private float normalFOV;

    [Header("Speedlines Follow")]
    [Tooltip("Référence à S_GunFollowCamera (sur ton effet speedlines)")]
    public S_GunFollowCamera gunFollowCamera;
    private float defaultDistance;

    // On passe en actif uniquement lorsque le vrai sprint démarre
    private bool isActive = false;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        initialSpawnRate = vfx.GetFloat("SpawnRate");
        initialSpeed = vfx.GetFloat("Speed");

        // Désactivé par défaut
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
    }

    private void Start()
    {
        // Récupère les valeurs de référence
        if (cinemachineCamera != null)
            normalFOV = cinemachineCamera.m_Lens.FieldOfView;
        if (gunFollowCamera != null)
            defaultDistance = gunFollowCamera.distanceFromCamera;

        // S’abonner aux événements de sprint
        var obs = S_PlayerStateObserver.Instance;
        obs.OnSprintStateEvent += HandleSprintState;
    }

    private void HandleSprintState(System.Enum stateEnum, int level)
    {
        // Supposons que level 1 = pas de sprint, level >=2 = sprint autorisé
        bool canSprint = level >= 2;

        switch (stateEnum)
        {
            case PlayerStates.SprintState.StartSprinting:
                if (canSprint)
                    ActivateSpeedlines();
                break;

            case PlayerStates.SprintState.StopSprinting:
                // dès que le sprint s’arrête (sol ou air),
                // on coupe à coup sûr
                DeactivateSpeedlines();
                break;
        }
    }

    private void ActivateSpeedlines()
    {
        isActive = true;
        vfx.SetFloat("SpawnRate", initialSpawnRate);
        vfx.SetFloat("Speed", initialSpeed);
        AdjustDistance(); // positionne tout de suite correctement
    }

    private void DeactivateSpeedlines()
    {
        isActive = false;
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
        // on laisse la distance ajustée pour les particules restantes
    }

    private void LateUpdate()
    {
        // Recalcule en permanence la distance pendant le sprint actif
        if (!isActive || cinemachineCamera == null || gunFollowCamera == null)
            return;

        AdjustDistance();
    }

    private void AdjustDistance()
    {
        float currentFOV = cinemachineCamera.m_Lens.FieldOfView;
        float halfNormRad = normalFOV * Mathf.Deg2Rad * 0.5f;
        float halfCurrentRad = currentFOV * Mathf.Deg2Rad * 0.5f;
        float ratio = Mathf.Tan(halfNormRad) / Mathf.Tan(halfCurrentRad);

        gunFollowCamera.distanceFromCamera = defaultDistance * ratio;
    }

    private void OnDisable()
    {
        if (S_PlayerStateObserver.Instance != null)
            S_PlayerStateObserver.Instance.OnSprintStateEvent -= HandleSprintState;
    }
}