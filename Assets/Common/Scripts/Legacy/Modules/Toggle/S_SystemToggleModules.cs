using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SystemToggleModules : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObjectGroup
    {
        public List<GameObject> toggleObjects = new List<GameObject>(); // Liste des objets à basculer
        public int resetInterval = 3; // Nombre de réinitialisations avant de basculer l'état des objets
    }

    public List<ToggleObjectGroup> toggleGroups = new List<ToggleObjectGroup>(); // Différents groupes d'objets avec des intervalles de basculement distincts

    private int currentResetCount = 0;

    public void IncrementResetCount()
    {
        // Incrémenter le compteur de réinitialisations
        currentResetCount++;

        // Vérifier si le nombre de réinitialisations a atteint l'intervalle de basculement pour chaque groupe
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
        // Basculer l'état de chaque objet du groupe
        foreach (GameObject obj in group.toggleObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
