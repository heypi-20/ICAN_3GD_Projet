
public static class PlayerStates
{
    public enum MoveState
    {
        StartMoving,
        IsMoving,
        StopMoving,
    }

    public enum SprintState
    {
        StartSprinting,
        IsSprinting,
        StopSprinting,
        SprintHit
    }

    public enum JumpState
    {
        Jump,
        OnGround,
        OnAir,
    }

    public enum ShootState
    {
        StartShoot,
        IsShooting,
        StopShoot,
        hitEnemy,
        hitWeakPoint
    }
    
    public enum LevelState
    {
        LevelUp,
        LevelDown,
        StartGrace,
        EndGrace,
    }

    public enum MeleeState
    {
        StartMeleeAttack,
        DashingBeforeMelee,
        MeleeAttackMissed,
        MeleeAttackHit,
        MeleeAttackHitWeakness,
        EndMeleeAttack,
    }

    public enum GroundPoundState
    {
        StartGroundPound,
        isGroundPounding,
        EndGroundPound,
    }

    public enum PlayerHealthState
    {
        PlayerGetHit,
        PlayerInvulnerabilityStart,
        PlayerIsInvulnerable,
        PlayerInvulnerabilityEnd,
        PlayerOneHitModeStart,
        PlayerInOneHitMode,
        PlayerOneHitModeEnd,
        PlayerIsDead
        
    }
    
}
