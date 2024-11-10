using System.Collections.Generic;
using UnityEngine;

public class S_PlayerToggleModules : MonoBehaviour
{
    [System.Serializable]
    public class ToggleObjectGroup
    {
        public List<GameObject> toggleObjects = new List<GameObject>(); // Liste des objets � basculer
        public int resetInterval = 3; // Nombre de r�initialisations avant de basculer l'�tat des objets
    }

    public List<ToggleObjectGroup> toggleGroups = new List<ToggleObjectGroup>(); // Diff�rents groupes d'objets avec des intervalles de basculement distincts

    private S_PlayerResetCounterModule playerResetCounterModule;
    private int previousResetCount = 0;

    private void Start()
    {
        // Obtenir la r�f�rence au module de compteur de r�initialisations du joueur
        playerResetCounterModule = GetComponent<S_PlayerResetCounterModule>();
        if (playerResetCounterModule != null)
        {
            previousResetCount = playerResetCounterModule.PlayerResetCount;
        }
    }

    private void OnPlayerReset()
    {
        // V�rifier si le nombre de r�initialisations a atteint l'intervalle de basculement pour chaque groupe
        int resetDifference = playerResetCounterModule.PlayerResetCount - previousResetCount;
        foreach (ToggleObjectGroup group in toggleGroups)
        {
            if (resetDifference >= group.resetInterval)
            {
                ToggleObjectsState(group);
            }
        }
        previousResetCount = playerResetCounterModule.PlayerResetCount; // Mettre � jour le compteur pr�c�dent
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
