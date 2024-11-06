using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class S_DamageModule : MonoBehaviour
{
    // Champs publics pour configurer le calcul des d�g�ts
    public float AdditionalDamage = 0f;   // Valeur de d�g�ts suppl�mentaires ajout�e aux d�g�ts de base
    public float DamageMultiplier = 1f;   // Multiplicateur appliqu� aux d�g�ts de base

    // �v�nement d�clench� apr�s le calcul des d�g�ts
    public event Action<float> OnDamage;

    // M�thode pour g�rer la r�ception des d�g�ts
    public void ReceiveDamage(float baseDamage)
    {
        // Calculer les d�g�ts finaux en utilisant AdditionalDamage et DamageMultiplier
        float finalDamage = (baseDamage + AdditionalDamage) * DamageMultiplier;
        Debug.Log("FinalDamage:" + finalDamage);
        // D�clencher l'�v�nement OnDamage avec la valeur des d�g�ts calcul�s
        OnDamage?.Invoke(finalDamage);
    }
}
