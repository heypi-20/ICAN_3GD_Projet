using UnityEngine;

public class S_Projectile : S_Weapon
{
    [Header("This weapon's properties")]
    public float fireRate;
    public float projectileSpeed;
    
    public override void Fire()
    {
        if (timer > fireRate && ammo > 0) {
            GameObject newProjectile = Instantiate(projectile, shootPoint.transform.position, transform.rotation);
            newProjectile.SetActive(true);
            newProjectile.GetComponent<Rigidbody>().velocity = transform.forward * projectileSpeed;
            timer = 0;
            ammo--;
        }
    }
}
