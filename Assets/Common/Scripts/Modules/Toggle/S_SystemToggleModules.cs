using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SystemToggleModules : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObjectGroup
    {
        public List<GameObject> toggleObjects = new List<GameObject>(); // Liste des objets � basculer
        public int resetInterval = 3; // Nombre de r�initialisations avant de basculer l'�tat des objets
    }

    public List<ToggleObjectGroup> toggleGroups = new List<ToggleObjectGroup>(); // Diff�rents groupes d'objets avec des intervalles de basculement distincts

    private int currentResetCount = 0;

    public void IncrementResetCount()
    {
        // Incr�menter le compteur de r�initialisations
        currentResetCount++;

        // V�rifier si le nombre de r�initialisations a atteint l'intervalle de basculement pour chaque groupe
        foreach (ToggleObjectGroup group in toggleGroups)
        {
            if (currentResetCount % group.resetInterval == 0)
            {
                ToggleObjectsState(group);
            }
        }
    }

    private void ToggleObjectsState(ToggleObjectGroup group)
    {
        // Basculer l'�tat de chaque objet du groupe
        foreach (GameObject obj in group.toggleObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
