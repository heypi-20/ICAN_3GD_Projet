using UnityEngine;

public abstract class S_Weapon : MonoBehaviour
{
    [Header("Weapon properties")]
    [Tooltip("The object to shoot")]
    public GameObject projectile;
    [Tooltip("Where the projectile is shot from")]
    public Transform shootPoint;
    [Tooltip("Weapon's damage")]
    public int weaponDmg;
    [Tooltip("Weapon's ammo")]
    public int ammo;
    
    // Timer for the fire rate
    protected float timer;

    private void Start()
    {
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    public virtual void Fire()
    {
        if (ammo <= 0) {
            Debug.Log("No ammo");
        }
    }
}
