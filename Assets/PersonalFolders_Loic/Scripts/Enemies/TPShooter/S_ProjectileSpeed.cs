using UnityEngine;

public class S_ProjectileSpeed : MonoBehaviour
{
    [HideInInspector]
    public float speed;
    
    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
    }
}
