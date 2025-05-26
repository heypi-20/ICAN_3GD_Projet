using System;
using UnityEngine;

public class S_TutoCheck2 : MonoBehaviour
{
    public int shootCount;
    public int meleeCount;
    
    public GameObject portal;

    private int sCount;
    private int mCount;
    private bool shootComplete;
    private bool meleeComplete;

    private bool once = false;

    private void OnEnable()
    {
        FindObjectOfType<S_FireRateGun_Module>().OnShootStateChange += CheckForShoot;
        FindObjectOfType<S_MeleeAttack_Module>().OnAttackStateChange += CheckForMelee;
    }

    private void Start()
    {
        portal.SetActive(false);
    }

    private void Update()
    {
        if (shootComplete && meleeComplete && !once) {
            portal.SetActive(true);
            once = true;
        }
    }

    private void CheckForShoot(Enum playerState, int level)
    {
        if (playerState.Equals(PlayerStates.ShootState.IsShooting))
            sCount++;

        if (sCount == shootCount)
            shootComplete = true;
    }

    private void CheckForMelee(Enum playerState, int level)
    {
        if (playerState.Equals(PlayerStates.MeleeState.EndMeleeAttack))
            mCount++;
        
        if (mCount == meleeCount)
            meleeComplete = true;
    }
}

