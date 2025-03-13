
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
        MeleeAttackMissed,
        MeleeAttackHit,
        EndMeleeAttack,
    }
    
}
