using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;

[RequireComponent(typeof(VisualEffect))]
public class S_SprintEffect : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Touche pour sprinter")]
    public KeyCode sprintKey = KeyCode.LeftShift;

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

    private bool isActive = false;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        // on lit la valeur d'origine
        initialSpawnRate = vfx.GetFloat("SpawnRate");
        initialSpeed = vfx.GetFloat("Speed");
        // on désactive tout au démarrage
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
    }

    private void Start()
    {
        // mémoriser le FOV "normal" et la distance à FOV normal (2.22 dans ton cas)
        if (cinemachineCamera != null)
            normalFOV = cinemachineCamera.m_Lens.FieldOfView;
        if (gunFollowCamera != null)
            defaultDistance = gunFollowCamera.distanceFromCamera;
    }

    private void Update()
    {
        // appui → on active les speedlines
        if (Input.GetKeyDown(sprintKey) && !isActive)
            ActivateSpeedlines();

        // relâchement → on désactive
        if (Input.GetKeyUp(sprintKey) && isActive)
            DeactivateSpeedlines();
    }

    private void ActivateSpeedlines()
    {
        isActive = true;
        vfx.SetFloat("SpawnRate", initialSpawnRate);
        vfx.SetFloat("Speed", initialSpeed);
        // on calcule tout de suite pour positionner correctement
        AdjustDistance();
    }

    private void DeactivateSpeedlines()
    {
        isActive = false;
        vfx.SetFloat("SpawnRate", 0f);
        vfx.SetFloat("Speed", 0f);
        // ne remet pas la distance à defaultDistance ici : on veut
        // voir les particules restantes se réajuster au FOV actuel
    }

    private void LateUpdate()
    {
        // tant que la caméra et le script de suivi sont branchés,
        // on recalculera la distance à chaque frame (même en ground-pound)
        if (cinemachineCamera == null || gunFollowCamera == null)
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
}