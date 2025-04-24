using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingEffect : MonoBehaviour
{

    // Référence au matériau de l'objet
    private Material objectMaterial;

    // Couleur de base de l'émissive
    public Color mycolor = Color.white;

    // Intensité maximale et minimale pour l'émissive
    public float maxEmissionIntensity = 20f;
    public float minEmissionIntensity = 5f;

    // Nom de la propriété émissive dans le shader (souvent "_EmissionColor")
    private static readonly string EmissionColorProperty = "_EmissionColor";

    // Variable pour stocker l'intensité actuelle
    private float currentIntensity;

    public S_PlayerStateObserver playerStateObserver;
    void Start()
    {
        // Récupérer le matériau du renderer de l'objet
        objectMaterial = GetComponent<Renderer>().material;

        // Définir l'intensité initiale (au départ, pas de clic)
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

        // Appliquer l'émissive avec la couleur et l'intensité actuelle
        objectMaterial.SetColor(EmissionColorProperty, mycolor * currentIntensity);
    }
}
