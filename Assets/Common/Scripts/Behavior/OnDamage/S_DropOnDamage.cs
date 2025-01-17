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
        // Ajouter automatiquement et r�f�rencer S_DamageModule et S_DroppingModule
        damageModule = GetComponent<S_DamageModule>();
        droppingModule = GetComponent<S_DroppingModule>();
        
    }

    private void OnEnable()
    {
        if (damageModule != null)
        {
            // �couter l'�v�nement OnDamage du module de d�g�ts
            damageModule.OnDamage += HandleOnDamage;
        }
    }

    private void OnDisable()
    {
        if (damageModule != null)
        {
            // Supprimer l'�coute de l'�v�nement OnDamage
            damageModule.OnDamage -= HandleOnDamage;
        }
    }

    private void HandleOnDamage(float damageAmount)
    {
        Debug.Log("�v�nement OnDamage d�tect�. L�cher des objets...");

        if (droppingModule != null)
        {
            // Appeler la m�thode DropItems du module de largage
            droppingModule.DropItems(0f);
        }
        else
        {
            Debug.LogWarning("Le module de largage n'est pas correctement assign�.");
        }
    }
}
