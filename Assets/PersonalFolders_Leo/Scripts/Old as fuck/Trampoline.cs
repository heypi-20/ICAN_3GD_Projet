using System;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    [Header("Transform")]
    public Transform _replacePoint;

    
    [Header("Force")]
    public float _forceBounce;
    public bool _forceEqualScale = false;
    public float _forceScale = 0f;
    public float _forceGrowth = 0.05f;
    
    [Header("Plant Apparence")]
    public float _scaleMax = 2f;
    public float _growthFactor;

    public bool _onlyUp;
    public Transform Top;
    public Transform Bottom;
    private void Update()
    {
        if (gameObject.transform.localScale.magnitude <= _scaleMax)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * _growthFactor , gameObject.transform.localScale.y * _growthFactor, gameObject.transform.localScale.z * _growthFactor);
            _forceScale += _forceGrowth;
        }

    }

    private void OnCollisionEnter(Collision other)
    {
        //other.transform.position = _replacePoint.position;
        
        if(_onlyUp == true)
        {
            if (_forceEqualScale)
            {
                other.rigidbody.AddForce(Vector3.up * _forceScale, ForceMode.Impulse);
            }
            else
            {
                other.rigidbody.AddForce(Vector3.up * _forceBounce, ForceMode.Impulse);
            }
        }

        if(_onlyUp == false)
        {
            Vector3 direction = Top.position - Bottom.position;
            Debug.Log(direction);
            if (_forceEqualScale)
            {
                other.rigidbody.AddForce(direction * _forceScale, ForceMode.Impulse);
            }
            else
            {
                other.rigidbody.AddForce(direction * _forceBounce, ForceMode.Impulse);
            }
        }
    }
}
