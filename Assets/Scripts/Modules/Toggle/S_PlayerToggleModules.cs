using System.Collections.Generic;
using UnityEngine;

public class S_PlayerToggleModules : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObjectGroup
    {
        public List<GameObject> toggleObjects = new List<GameObject>(); // Liste des objets à basculer
        public int resetInterval = 3; // Nombre de réinitialisations avant de basculer l'état des objets
    }

    public List<ToggleObjectGroup> toggleGroups = new List<ToggleObjectGroup>(); // Différents groupes d'objets avec des intervalles de basculement distincts

    private S_PlayerResetCounterModule playerResetCounterModule;
    private int previousResetCount = 0;

    private void Start()
    {
        // Obtenir la référence au module de compteur de réinitialisations du joueur
        playerResetCounterModule = GetComponent<S_PlayerResetCounterModule>();
        if (playerResetCounterModule != null)
        {
            previousResetCount = playerResetCounterModule.PlayerResetCount;
        }
    }

    private void OnPlayerReset()
    {
        // Vérifier si le nombre de réinitialisations a atteint l'intervalle de basculement pour chaque groupe
        int resetDifference = playerResetCounterModule.PlayerResetCount - previousResetCount;
        foreach (ToggleObjectGroup group in toggleGroups)
        {
            if (resetDifference >= group.resetInterval)
            {
                ToggleObjectsState(group);
            }
        }
        previousResetCount = playerResetCounterModule.PlayerResetCount; // Mettre à jour le compteur précédent
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
