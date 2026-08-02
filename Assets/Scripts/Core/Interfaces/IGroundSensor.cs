namespace Core.Interfaces
{
    // Hợp đồng này chỉ hứa đúng 1 điều: "Tôi biết tôi có đang chạm đất hay không"
    public interface IGroundSensor
    {
        bool IsGrounded { get; }
    }
}