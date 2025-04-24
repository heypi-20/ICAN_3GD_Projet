using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEffect : MonoBehaviour
{

    // R�f�rence au mat�riau de l'objet
    private Material objectMaterial;

    // Couleur de base de l'�missive
    public Color mycolor = Color.white;

    // Intensit� maximale et minimale pour l'�missive
    public float maxEmissionIntensity = 20f;
    public float minEmissionIntensity = 5f;

    // Nom de la propri�t� �missive dans le shader (souvent "_EmissionColor")
    private static readonly string EmissionColorProperty = "_EmissionColor";

    // Variable pour stocker l'intensit� actuelle
    private float currentIntensity;

    public S_PlayerStateObserver playerStateObserver;
    void Start()
    {
        // R�cup�rer le mat�riau du renderer de l'objet
        objectMaterial = GetComponent<Renderer>().material;

        // D�finir l'intensit� initiale (au d�part, pas de clic)
        currentIntensity = minEmissionIntensity;
    }

    void Update()
    {
        
        //if (playerStateObserver.PlayerStates.ShootState.IsShooting)
        //{
        //    currentIntensity = Mathf.Lerp(currentIntensity, maxEmissionIntensity, Time.deltaTime * 5f);
        //}
        //else
        //{
        //    currentIntensity = Mathf.Lerp(currentIntensity, minEmissionIntensity, Time.deltaTime * 5f);
        //}

        // Appliquer l'�missive avec la couleur et l'intensit� actuelle
        objectMaterial.SetColor(EmissionColorProperty, mycolor * currentIntensity);
    }
}
