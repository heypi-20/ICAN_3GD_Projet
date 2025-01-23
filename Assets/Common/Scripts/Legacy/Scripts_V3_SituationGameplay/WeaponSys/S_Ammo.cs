using UnityEngine;

public class S_Ammo : MonoBehaviour
{
    private S_WeaponSys weaponSys;

    // Start is called before the first frame update
    void Start()
    {
        weaponSys = GetComponent<S_WeaponSys>();
    }

    // Get a ammo when colliding with an object that has the "Cube" Tag
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cube")) {
            Destroy(other.gameObject);
            weaponSys.currentWeapon.ammo++;
        }
    }
}
