namespace Core.Interfaces
{
    public interface IMovementStats
    {
        float MoveSpeed { get; }
        float JumpForce { get; }
        float AirControlMultiplier { get; }

        float CoyoteTime { get; }
        float JumpBufferTime { get; }
        float JumpCutMultiplier { get; }
    }
}