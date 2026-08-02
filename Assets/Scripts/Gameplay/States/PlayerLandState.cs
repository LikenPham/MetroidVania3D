using Core.DataModels;
using Core.Events;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerLandState : PlayerState
    {
        private int landHash;

        private float lockoutTimer;
        private const float hardLandLockout = 0.12f;
        public PlayerLandState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            landHash = stateMachine.AnimData.GetHash(AnimActionKey.Land);
        }

        public override void Enter()
        {
            base.Enter();

            lockoutTimer = hardLandLockout;

            // 1. Phanh gấp trục X để tạo độ nặng khi rơi xuống
            stateMachine.MoveModule.StopImmediately();

            // 2. Rút thẻ nhớ lấy mã băm hoạt ảnh "Land" ném vào đầu đĩa
            stateMachine.AnimModule.PlayAnimation(landHash);

            EventManager.Broadcast(new PlayerLandedEvent(stateMachine.transform.position.y));
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            stateMachine.MoveModule.SetMoveInput(0f);

            lockoutTimer -= Time.deltaTime;

            if (stateMachine.InputReader.JumpTriggered && stateMachine.MoveModule.IsGrounded)
            {
                stateMachine.InputReader.ConsumeJumpInput();

                // VÁ LỖI QUÊN CHÌA KHÓA: Phải gửi giấy nợ cho Tài xế!
                stateMachine.MoveModule.TriggerJump();

                stateMachine.ChangeState(stateMachine.JumpState);
                return;
            }

            if (lockoutTimer > 0f) return;

            float moveInput = stateMachine.InputReader.MoveDirection.x;
            if (Mathf.Abs(moveInput) >= 0.1f)
            {
                stateMachine.ChangeState(stateMachine.RunState);
                return;
            }

            // QUYỀN LỰC 3: Hết hoạt ảnh -> Về Idle
            if (stateMachine.AnimModule.IsAnimationFinished(landHash))
            {
                stateMachine.ChangeState(stateMachine.IdleState);
                return;
            }

        }
    }
}