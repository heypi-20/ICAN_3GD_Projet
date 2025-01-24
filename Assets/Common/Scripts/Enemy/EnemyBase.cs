using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public string enemyName;
    public float health;
    public GameObject energyPoint;
    public float energyDropQuantity;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public string enemyGetHitSound;
    public string enemyDeathSound;
    
    private bool isDead = false;


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
        
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        DropItems(DropBonus);
        Destroy(gameObject,5f);
    }
    
    private void DropItems(float DropBonus)
    {
        for (int i = 0; i < energyDropQuantity+DropBonus; i++)
        {
           
            Instantiate(energyPoint, transform.position, Quaternion.identity);
        }
    }
    
}
