using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossColor : MonoBehaviour
{
    public Image CrossAir;
    public Color ColorCrossAir;
    public Camera mainCamera; // La cam�ra principale, � assigner dans l'inspecteur
    public float maxDistance = Mathf.Infinity; // Distance maximale du Raycast
    public LayerMask layerMask; // Optionnel : filtre les layers � v�rifier

    void Update()
    {
        // V�rifie si une cam�ra est assign�e
        if (mainCamera == null)
        {
            Debug.LogWarning("Aucune cam�ra assign�e !");
            return;
        }

        // Cr�e un Ray partant de la cam�ra
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Informations sur le Raycast
        RaycastHit hit;

        // Lance le Raycast
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            // R�cup�re l'objet touch�
            GameObject hitObject = hit.collider.gameObject;

            // R�cup�re le layer de l'objet touch�
            int hitLayer = hitObject.layer;

            // Affiche les informations dans la console
            //Debug.Log($"Raycast a touch� : {hitObject.name} sur le layer {LayerMask.LayerToName(hitLayer)} (ID: {hitLayer})");
            CrossAir.color = ColorCrossAir;
        }
        else
        {
            //Debug.Log("Raycast n'a touch� aucun objet.");
            CrossAir.color = Color.white;
        }
    }
}
