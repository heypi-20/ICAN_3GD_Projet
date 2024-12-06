using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class centre_noeud : MonoBehaviour
{
    public Jump_Noeud jumpnoeud;
    public float add_bullet_to_timer = 2;
    public float add_bullet_to_jump = 1;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            jumpnoeud.time_to_survive = jumpnoeud.time_to_survive + add_bullet_to_timer;
            jumpnoeud.Jump_force = jumpnoeud.Jump_force + add_bullet_to_jump;
            Destroy(other.gameObject);
        }
    }
}
