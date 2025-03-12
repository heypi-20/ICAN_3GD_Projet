
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
    
}
