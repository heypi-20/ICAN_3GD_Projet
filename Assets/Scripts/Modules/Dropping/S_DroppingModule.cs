using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class S_DroppingModule : MonoBehaviour
{
    [System.Serializable]
    public class DropItem
    {
        public GameObject item; // Préfabriqué de l'objet à faire tomber
        public int quantity = 1; // Quantité d'objets à faire tomber
        public float explosionForce = 5f; // Force de l'explosion simulant l'effet de chute
        public float explosionRadius = 3f; // Rayon de l'explosion
    }

    public List<DropItem> dropItems = new List<DropItem>(); // Liste des objets à faire tomber
    public event Action OnDropping; // Interface d'événement de chute, réservée à une utilisation future

    private bool hasDropped = false; // Empêcher les chutes multiples dans une seule frame

    // Méthode pour faire tomber les objets, à appeler de l'extérieur
    public void DropItems()
    {
        if (hasDropped) return; // Si les objets ont déjà été lâchés, ne rien faire

        foreach (DropItem dropItem in dropItems)
        {
            for (int i = 0; i < dropItem.quantity; i++)
            {
                // Instancier l'objet à faire tomber
                GameObject droppedItem = Instantiate(dropItem.item, transform.position, Random.rotation);

                // Ajouter un effet physique (explosion) pour simuler une chute réaliste
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(dropItem.explosionForce, transform.position, dropItem.explosionRadius, 1f, ForceMode.Impulse);
                }
            }
        }

        // Déclencher l'événement de chute
        OnDropping?.Invoke(); // Déclencher l'événement de chute

        hasDropped = true; // Marquer que les objets ont été lâchés

        // Démarrer une coroutine pour réinitialiser le compteur `hasDropped`
        StartCoroutine(ResetHasDropped());
    }

    // Coroutine pour réinitialiser `hasDropped` après une certaine durée
    private System.Collections.IEnumerator ResetHasDropped()
    {
        yield return new WaitForSeconds(0.05f); // Attendre 0.05 seconde
        hasDropped = false; // Réinitialiser le compteur pour permettre une nouvelle chute
    }
}
