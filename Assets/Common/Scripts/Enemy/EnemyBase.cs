using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public string enemyName;

    public float enemyDamage;
    public float health;
    public GameObject energyPoint;
    public float energyDropQuantity;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public string enemyGetHitSound;
    public string enemyDeathSound;
    
    private bool isDead = false;
    private S_ScoreDisplay _s_ScoreDisplay;

    public void ReduceHealth(float amount,int DropBonus)
    {
        if(isDead) return;
        health -= amount;
        
        //placeholder pour le VFX =========
        if (enemyGetHitVFX is not null)
        {
            GameObject GetHitVFX = Instantiate(enemyGetHitVFX, transform.position, transform.rotation);
            Destroy(GetHitVFX,3f);
        }
        //placeholder pour le VFX ========
        if (health <= 0)
        {
            EnemyDied(DropBonus);
        }
    }

    public void EnemyDied(int DropBonus)
    {
        isDead = true;
        if (enemyDeathVFX is not null)
        {
            //placeholder pour le VFX =======
            GameObject DeathVFX = Instantiate(enemyDeathVFX, transform.position, transform.rotation);
            Destroy(DeathVFX,3f);
            //placeholder pour le VFX =======
        }
        
        SoundManager.Instance.Meth_Shoot_Kill(1);
        DropItems(DropBonus);
        Destroy(gameObject,5f);
        _s_ScoreDisplay = FindObjectOfType<S_ScoreDisplay>();
        _s_ScoreDisplay.AddScore(20f);
        gameObject.SetActive(false);
    }
    
    private void DropItems(float DropBonus)
    {
        for (int i = 0; i < energyDropQuantity+DropBonus; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f), 
                Random.Range(-0.5f, 0.5f)  
            );
            Vector3 spawnPosition = transform.position + randomOffset;
            GameObject point = Instantiate(energyPoint, spawnPosition, Quaternion.identity);
            Vector3 pointdirection = point.transform.position - transform.position;
            pointdirection.Normalize();
            point.GetComponent<Rigidbody>().AddForce(pointdirection*15f,ForceMode.Impulse);
        }
    }
    
}
