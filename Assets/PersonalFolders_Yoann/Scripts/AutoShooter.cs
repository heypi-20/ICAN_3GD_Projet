using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public float _fireRate = 1.5f;
    public Transform _shootPoint;
    public GameObject _objectToShoot;
    public float _launchVelocity = 20f;
    
    private float t; // Le temps actuel du Timer
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        
        if(t >= _fireRate)
        {
            t = 0f;
            //Todo : logique à réaliser quand le temps est écoulé
            GameObject projectile = Instantiate(_objectToShoot, _shootPoint.position,_shootPoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.AddForce(_shootPoint.forward * _launchVelocity, ForceMode.Impulse);
            
        }
    }
}
