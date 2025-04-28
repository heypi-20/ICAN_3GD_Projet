using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundEditorColor : MonoBehaviour
{
    public Color gizmoColor;
    
    private void OnDrawGizmos()
    {
        // Change la couleur du gizmo
        Gizmos.color = gizmoColor;

        // Récupère le Collider attaché à cet objet
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            // Dessine un cube aux dimensions du collider
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
        }
        else
        {
            // Message d'avertissement si aucun collider n'est présent
            Debug.LogWarning("Aucun collider n'est attaché à cet objet.");
        }
    }
}
