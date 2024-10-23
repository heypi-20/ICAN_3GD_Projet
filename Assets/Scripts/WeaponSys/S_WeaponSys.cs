using System;
using UnityEngine;

public class S_WeaponSys : MonoBehaviour
{
    [Header("Weapon system properties")]
    [Tooltip("Player weapons")]
    public S_Weapon[] weapons;
    [Tooltip("Weapon's number in the list")]
    public int weaponID = 0;
    [Tooltip("Starting weapon's number in the list")]
    public int startingWeapon = 0;
    
    [Header("Keybindings")]
    [Tooltip("Key to shoot")]
    public KeyCode shootKey = KeyCode.E;
    [Tooltip("Key to change to the next weapon of the list")]
    public KeyCode weaponKey = KeyCode.R;
    
    [Header("Read-only properties")]
    [Tooltip("Read-only showing player's current weapon, DO NOT MODIFY")]
    public S_Weapon currentWeapon;
    
    private void Start()
    {
        if (weapons.Length == 0) {
            Debug.LogError("No weapons defined!");
        } else {
            currentWeapon = weapons[startingWeapon];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(weaponKey)) {
            weaponID++;
            weaponID %= weapons.Length;
            currentWeapon = weapons[weaponID];
        }
        if (Input.GetKey(shootKey)) {
            currentWeapon.Fire();
        }
    }
}
