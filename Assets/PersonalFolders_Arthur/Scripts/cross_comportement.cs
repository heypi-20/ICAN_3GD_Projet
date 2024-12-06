using UnityEngine;

public class cross_comportement : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("FirstCollider");
        
        if (other.gameObject.CompareTag("Ennergie_Source"))
        {
            Debug.Log("InsideCollider");
            
            GameManager.instance.AddEnergyPointOnCross();
            
            Destroy(other.gameObject);
        }
    }
}