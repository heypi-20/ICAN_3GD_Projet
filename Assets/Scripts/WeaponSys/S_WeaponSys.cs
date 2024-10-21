using System;
using UnityEngine;

public class S_WeaponSys : MonoBehaviour
{
    public S_Weapon[] weapons;

    private S_Weapon currentWeapon;

    private void Start()
    {
        currentWeapon = weapons[0];
        currentWeapon.Fire();
    }
}
