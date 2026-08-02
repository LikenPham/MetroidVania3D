namespace Gameplay.StateMachine
{
    // Lớp trừu tượng (Abstract), không kế thừa MonoBehaviour vì nó không gắn lên GameObject
    public abstract class PlayerState
    {
        // Protected để các class con (Idle, Run) có thể sử dụng
        protected PlayerStateMachine stateMachine;

        // Constructor bắt buộc: Khi sinh ra phải biết "Não bộ" của mình là ai
        public PlayerState(PlayerStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }

        // Chạy trong Update() - Dành cho Input, đếm ngược thời gian, chuyển trạng thái
        public virtual void LogicUpdate() { }

        // Chạy trong FixedUpdate() - Dành cho các lệnh ép lực, di chuyển Rigidbody
        public virtual void PhysicsUpdate() { }

        public virtual void Exit() { }
    }
}