using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcSimple : MonoBehaviour
{
    public GameObject target; // Le point vers lequel le NPC doit se d�placer
    private NavMeshAgent agent;
    public int damage;
    public bool isAngwy;
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        // R�cup�re le composant NavMeshAgent attach� au GameObject
        agent = GetComponent<NavMeshAgent>();

        if (target == null)
        {
            Debug.LogError("Aucun point cible assign� au NPC.");
        }
    }

    void Update()
    {
        if (target != null)
        {
            // D�place le NPC vers la cible
            agent.SetDestination(target.transform.position);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(isAngwy == true)
        {
            if (collision.collider.CompareTag("Player"))
            {
                if (GameManager.instance.energyPoints > damage)
                {
                    GameManager.instance.energyPoints -= damage;
                    GameManager.instance.UpdateEnergyUI();
                }
                Destroy(gameObject);
            }
        }
    }
}
