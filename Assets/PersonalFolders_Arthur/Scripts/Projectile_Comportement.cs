using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Comportement : MonoBehaviour
{
    public float speed = 10f; // Vitesse du projectile
    public float lifetime = 5f; // Durée avant destruction

    private void Start()
    {
        // Détruire le projectile après un certain temps
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Déplacer le projectile tout droit dans la direction locale "forward"
        transform.position += transform.forward * speed * Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Vérifie si l'objet touché a le tag "Energie_Source"
        if (collision.gameObject.CompareTag("Ennergie_Source"))
        {
            // Incrémente la variable d'énergie via le GameManager
            GameManager.instance.AddEnergyPointOnDestroy();

            // Détruit l'objet touché
            Destroy(collision.gameObject);

            // Détruit le projectile
            Destroy(gameObject);

            Debug.Log("Source d'énergie détruite !");
        }
    }
}
