using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_DestructibleWalls : MonoBehaviour
{
    public string targetTag = "Player";
    public GameObject destructionEffect;
    public GameObject resetPrefab;
    public GameObject wallPrefab;
    public int regrowThreshold = 3;

    // Liste pour enregistrer les positions des murs détruits, la ronde de destruction et si resetPrefab a été généré
    private List<(Vector3 position, int destroyedRound, bool hasResetPrefab)> destroyedWalls = new List<(Vector3, int, bool)>();

    // Liste pour enregistrer les particules actives
    private List<GameObject> activeEffects = new List<GameObject>();

    private int currentResetRound = 0;

    // S'abonner à l'événement de réinitialisation lors de l'activation
    void OnEnable()
    {
        S_ZoneResetSysteme.OnZoneReset += ResetDestroyedWalls;
    }

    // Se désabonner de l'événement de réinitialisation lors de la désactivation
    void OnDisable()
    {
        S_ZoneResetSysteme.OnZoneReset -= ResetDestroyedWalls;
    }

    // Déclenché lors de la collision avec un sous-objet
    void OnCollisionEnter(Collision collision)
    {
        // Vérifier si l'objet en collision a le bon tag
        if (collision.gameObject.CompareTag(targetTag))
        {
            // Obtenir le mur touché (sous-objet)
            GameObject hitWall = collision.contacts[0].thisCollider.gameObject;

            // Détruire le mur et enregistrer sa position
            if (hitWall != this.gameObject)
            {
                DestroyWall(hitWall);
                Debug.Log($"{hitWall.name} a été détruit.");
            }
        }
    }

    // Détruire le mur touché et générer un effet de particule
    private void DestroyWall(GameObject wall)
    {
        // Obtenir la position du mur détruit
        Vector3 destructionPosition = wall.transform.position;

        // Enregistrer la position du mur, le nombre de réinitialisations et marquer que resetPrefab n'a pas encore été généré
        destroyedWalls.Add((destructionPosition, currentResetRound, false));

        // Détruire le mur
        Destroy(wall);

        // Générer les particules si nécessaire et enregistrer l'effet
        if (destructionEffect != null)
        {
            GameObject effectInstance = Instantiate(destructionEffect, destructionPosition, Quaternion.identity);
            activeEffects.Add(effectInstance);  
        }
    }

    // Gérer la logique de réinitialisation des murs
    public void ResetDestroyedWalls()
    {
        // Incrémenter le nombre de réinitialisations
        currentResetRound++;

        // Liste temporaire pour enregistrer les murs à reconstruire
        List<Vector3> wallsToRegrow = new List<Vector3>();

        // Parcourir la liste des murs détruits
        for (int i = 0; i < destroyedWalls.Count; i++)
        {
            var destroyedWall = destroyedWalls[i];

            // Si le mur a attendu suffisamment de réinitialisations
            if (currentResetRound - destroyedWall.destroyedRound >= regrowThreshold)
            {
                wallsToRegrow.Add(destroyedWall.position);
            }
            else
            {
                // Générer resetPrefab uniquement lors de la première réinitialisation
                if (!destroyedWall.hasResetPrefab)
                {
                    Instantiate(resetPrefab, destroyedWall.position, Quaternion.identity);

                    // Marquer que resetPrefab a été généré
                    destroyedWalls[i] = (destroyedWall.position, destroyedWall.destroyedRound, true);
                }
            }
        }

        // Reconstruire les murs et gérer les effets
        RegrowWalls(wallsToRegrow);
    }

    // Reconstruire les murs qui ont attendu suffisamment longtemps et détruire les effets associés
    private void RegrowWalls(List<Vector3> wallsToRegrow)
    {
        foreach (var position in wallsToRegrow)
        {
            // Générer les murs au même endroit
            Instantiate(wallPrefab, position, Quaternion.identity, this.transform);

            // Rechercher et détruire l'effet correspondant à cette position
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i] != null && activeEffects[i].transform.position == position)
                {
                    Destroy(activeEffects[i]);
                    activeEffects.RemoveAt(i); // Retirer de la liste après destruction
                    break;
                }
            }

            // Retirer les murs reconstruits de la liste
            destroyedWalls.RemoveAll(wall => wall.position == position);
        }
    }
}
