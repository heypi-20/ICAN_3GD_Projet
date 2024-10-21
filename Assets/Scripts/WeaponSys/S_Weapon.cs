using System;
using UnityEngine;

public class S_Weapon : MonoBehaviour
{
    [Header("Weapon properties")]
    public GameObject projectile;
    public Transform shootPoint;
    public int weaponDmg;

    [Header("Keybindings")]
    [Tooltip("Touche pour tirer")]
    public KeyCode shootKey;
    public KeyCode defaultShootKey = KeyCode.E;
    
    private S_Ammo ammo;
    
    private void Start()
    {
        ammo = GetComponent<S_Ammo>();
        
        if(shootKey == KeyCode.None) {
            shootKey = defaultShootKey;
        }
    }

    private void Update()
    {
        if(ammo.ammo > 0) {
            Fire();
        }
    }

    public void Fire()
    {
        if(Input.GetKeyDown(shootKey)) {
            GameObject newProjectile = Instantiate(projectile, shootPoint.transform.position, transform.rotation);
            newProjectile.SetActive(true);
            newProjectile.GetComponent<Rigidbody>().velocity = transform.forward * 100f;
            ammo.ammo--;
        }
    }
}