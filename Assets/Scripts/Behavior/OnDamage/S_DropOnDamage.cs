using UnityEngine;
using System;

[RequireComponent(typeof(S_DamageModule))]
[RequireComponent(typeof(S_DroppingModule))]
public class S_DropOnDamage : MonoBehaviour
{
    private S_DamageModule damageModule;
    private S_DroppingModule droppingModule;

    private void Awake()
    {
        // Ajouter automatiquement et référencer S_DamageModule et S_DroppingModule
        damageModule = GetComponent<S_DamageModule>();
        droppingModule = GetComponent<S_DroppingModule>();
    }

    private void OnEnable()
    {
        if (damageModule != null)
        {
            // Écouter l'événement OnDamage du module de dégâts
            damageModule.OnDamage += HandleOnDamage;
        }
    }

    private void OnDisable()
    {
        if (damageModule != null)
        {
            // Supprimer l'écoute de l'événement OnDamage
            damageModule.OnDamage -= HandleOnDamage;
        }
    }

    private void HandleOnDamage(float damageAmount)
    {
        Debug.Log("Événement OnDamage détecté. Lâcher des objets...");

        if (droppingModule != null)
        {
            // Appeler la méthode DropItems du module de largage
            droppingModule.DropItems();
        }
        else
        {
            Debug.LogWarning("Le module de largage n'est pas correctement assigné.");
        }
    }
}
