using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class S_DamageModule : MonoBehaviour
{
    // Champs publics pour configurer le calcul des dégâts
    public float AdditionalDamage = 0f;   // Valeur de dégâts supplémentaires ajoutée aux dégâts de base
    public float DamageMultiplier = 1f;   // Multiplicateur appliqué aux dégâts de base

    // Événement déclenché après le calcul des dégâts
    public event Action<float> OnDamage;

    // Méthode pour gérer la réception des dégâts
    public void ReceiveDamage(float baseDamage)
    {
        // Calculer les dégâts finaux en utilisant AdditionalDamage et DamageMultiplier
        float finalDamage = (baseDamage + AdditionalDamage) * DamageMultiplier;
        Debug.Log("FinalDamage:" + finalDamage);
        // Déclencher l'événement OnDamage avec la valeur des dégâts calculés
        OnDamage?.Invoke(finalDamage);
    }
}
