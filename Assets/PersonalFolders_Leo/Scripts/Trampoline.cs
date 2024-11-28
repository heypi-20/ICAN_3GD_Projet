using System;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public Transform _replacePoint;
    public float _forceBounce;
    public float _growthFactor;
    public float _scaleMax = 2f;
    private float _forceScale = 0f;
    public bool _forceEqualScale = false;

    void Start()
    {
        
    }
    
    private void Update()
    {
        if (gameObject.transform.localScale.magnitude <= _scaleMax)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * _growthFactor , gameObject.transform.localScale.y * _growthFactor, gameObject.transform.localScale.z * _growthFactor);
        }

    }

    private void OnCollisionEnter(Collision other)
    {
        other.transform.position = _replacePoint.position;
        other.rigidbody.AddForce(Vector3.up * _forceBounce, ForceMode.Impulse);
    }
}
