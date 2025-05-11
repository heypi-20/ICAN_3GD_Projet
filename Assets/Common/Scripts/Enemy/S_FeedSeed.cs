using UnityEngine;

public class S_FeedSeed : EnemyBase
{
    public float respawnCoolDown = 5f;
    // track previous health to detect drops
    private float previousHealth;

    private void Start()
    {
        // initialize previousHealth from base class's currentHealth
        previousHealth = currentHealth;
    }

    private void OnDisable()
    {
        Debug.Log("StartRespawn");
        // schedule re-enable after cooldown
        Invoke(nameof(ReEnableSelf), respawnCoolDown);
    }

    private void ReEnableSelf()
    {
        gameObject.SetActive(true);
        // reset previousHealth so we don't immediately drop again
        previousHealth = currentHealth;
    }

    private void Update()
    {
        // detect any health decrease since last frame
        if (currentHealth < previousHealth)
        {
            // spawn configured number of energy points
            DropItems(0);
            // update tracker
            previousHealth = currentHealth;
        }
    }
}