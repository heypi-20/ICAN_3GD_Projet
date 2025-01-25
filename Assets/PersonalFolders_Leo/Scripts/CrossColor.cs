using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossColor : MonoBehaviour
{
    public Image CrossAir;
    public Color ColorCrossAir;
    public Camera mainCamera; // La caméra principale, à assigner dans l'inspecteur
    public float maxDistance = Mathf.Infinity; // Distance maximale du Raycast
    public LayerMask layerMask; // Optionnel : filtre les layers à vérifier

    void Update()
    {
        // Vérifie si une caméra est assignée
        if (mainCamera == null)
        {
            Debug.LogWarning("Aucune caméra assignée !");
            return;
        }

        // Crée un Ray partant de la caméra
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Informations sur le Raycast
        RaycastHit hit;

        // Lance le Raycast
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            // Récupère l'objet touché
            GameObject hitObject = hit.collider.gameObject;

            // Récupère le layer de l'objet touché
            int hitLayer = hitObject.layer;

            // Affiche les informations dans la console
            //Debug.Log($"Raycast a touché : {hitObject.name} sur le layer {LayerMask.LayerToName(hitLayer)} (ID: {hitLayer})");
            CrossAir.color = ColorCrossAir;
        }
        else
        {
            //Debug.Log("Raycast n'a touché aucun objet.");
            CrossAir.color = Color.white;
        }
    }
}
