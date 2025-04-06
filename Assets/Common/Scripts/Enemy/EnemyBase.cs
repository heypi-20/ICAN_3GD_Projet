using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : MonoBehaviour
{
    [Header("Enemy Infos")]
    public string enemyName;
    public EnemyType enemyType; // ✅ 新增：用于 Tracker 和 Pool 分类
    public float enemyDamage;
    public float health;
    
    public float WeaknessExposureHealth;
    public GameObject WeakPoint;
    public GameObject energyPoint;
    public float energyDropQuantity;
    public GameObject enemyGetHitVFX;
    public GameObject enemyDeathVFX;
    public string enemyGetHitSound;
    public string enemyDeathSound;

    private float currentHealth;
    private bool isDead = false;
    private S_ScoreDisplay _s_ScoreDisplay;

    public static event Action OnEnemyKillForCombo; // ✅ 保留你已有的全局事件
    public event Action<EnemyBase> OnKilled; // ✅ 新增：实例事件，用于池化系统

    private void OnEnable()
    {
        FindWeakPoint();
        currentHealth = health;
        isDead = false; // ✅ 重置死亡状态（用于对象池复用）
    }

    private void FindWeakPoint()
    {
        Transform[] children = transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.CompareTag("WeakPoint"))
            {
                WeakPoint = child.gameObject;
                WeakPoint.SetActive(false);
                break;
            }
        }
    }

    public void ReduceHealth(float amount, int DropBonus)
    {
        if (isDead) return;

        currentHealth -= amount;

        // VFX
        if (enemyGetHitVFX != null)
        {
            GameObject GetHitVFX = Instantiate(enemyGetHitVFX, transform.position, transform.rotation);
            Destroy(GetHitVFX, 3f);
        }

        if (currentHealth <= WeaknessExposureHealth && WeakPoint != null)
        {
            WeakPoint.SetActive(true);
        }

        if (currentHealth <= 0)
        {
            EnemyDied(DropBonus);
        }
    }

    public void EnemyDied(int DropBonus)
    {
        if (isDead) return;
        isDead = true;

        // Death VFX
        if (enemyDeathVFX != null)
        {
            GameObject DeathVFX = Instantiate(enemyDeathVFX, transform.position, transform.rotation);
            Destroy(DeathVFX, 3f);
        }

        OnEnemyKillForCombo?.Invoke();                 // ✅ 你的原有事件
        OnKilled?.Invoke(this);                // ✅ 实例事件，适配对象池
        SoundManager.Instance.Meth_Shoot_Kill(1);
        DropItems(DropBonus);

        gameObject.SetActive(false);           // ✅ 关闭对象，池系统会处理回收
        
    }

    private void DropItems(float DropBonus)
    {
        for (int i = 0; i < energyDropQuantity + DropBonus; i++)
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
            point.GetComponent<Rigidbody>().AddForce(pointdirection * 10f, ForceMode.Impulse);
        }
    }

    private void OnDisable()
    {
        OnKilled = null; // ✅ 避免事件多次订阅
    }
}
