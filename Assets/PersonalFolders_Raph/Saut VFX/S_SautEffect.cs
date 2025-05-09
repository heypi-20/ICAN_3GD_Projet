using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class S_SautEffect : MonoBehaviour
{
    [Header("Références VFX")]
    [Tooltip("Prefab du cercle au sol (quad avec MAT_Saut)")]
    public GameObject circlePrefab;
    [Tooltip("Prefab du tube lumineux (cylindre + shader HaloTube)")]
    public GameObject haloPrefab;

    private S_SuperJump_Module _jumpModule;

    void Awake()
    {
        _jumpModule = GetComponent<S_SuperJump_Module>();
        if (_jumpModule == null)
        {
            Debug.LogError("S_SautEffect: pas de S_SuperJump_Module trouvé !");
            enabled = false;
            return;
        }
    }

    void OnEnable()
    {
        _jumpModule.OnJumpStateChange += OnJumpStateChange;
    }

    void OnDisable()
    {
        _jumpModule.OnJumpStateChange -= OnJumpStateChange;
    }

    private void OnJumpStateChange(Enum state)
    {
        if (state is PlayerStates.JumpState js && js == PlayerStates.JumpState.Jump)
            ShowFeedback();
    }

    private void ShowFeedback()
    {
        Vector3 basePos = transform.position;
        // On récupère la portée du vortex pour dimensionner l'effet
        float radius = _jumpModule.vortexRadius;
        float duration = _jumpModule.vortexDuration;

        // --- Cercle au sol ---
        Vector3 circlePos = basePos + Vector3.up * 0.05f;
        GameObject circle = Instantiate(circlePrefab, circlePos, Quaternion.Euler(90, 0, 0));
        float diameter = radius * 2f;
        circle.transform.localScale = new Vector3(diameter, diameter, 1f);
        Destroy(circle, duration);

        // --- Tube lumineux (halo) ---
        // On utilise radius pour hauteur aussi (effet proportionnel)
        float height = radius;
        float halfHeight = height * 0.5f;

        Vector3 haloPos = basePos + Vector3.up * halfHeight;
        GameObject halo = Instantiate(haloPrefab, haloPos, Quaternion.identity);
        // Unity Cylinder par défaut: hauteur = 2 unités, donc scale.y = height/2
        halo.transform.localScale = new Vector3(diameter, halfHeight, diameter);

        // Met à jour les paramètres du shader pour le gradient vertical
        Renderer rend = halo.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = rend.material;
            mat.SetFloat("_MinY", basePos.y);
            mat.SetFloat("_MaxY", basePos.y + height);
        }

        Destroy(halo, duration);
    }
}