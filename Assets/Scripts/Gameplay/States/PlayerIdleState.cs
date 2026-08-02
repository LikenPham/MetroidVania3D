using Core.DataModels;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerIdleState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float FallGraceTime = 0.06f;

        private int idleHash;
        public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            idleHash = stateMachine.AnimData.GetHash(AnimActionKey.Idle);
        }

        public override void Enter()
        {
            stateMachine.AnimModule.PlayAnimation(idleHash);
            // Đảm bảo vừa vào Idle là nhân vật phanh lại đứng im luôn (tránh trượt băng)
            stateMachine.MoveModule.StopImmediately();
        }

        public override void LogicUpdate()
        {
            // Lấy trục X của tay cầm/bàn phím
            float moveInput = stateMachine.InputReader.MoveDirection.x;

            stateMachine.MoveModule.SetMoveInput(0f);

            // Nhảy tại chỗ
            if (stateMachine.InputReader.JumpTriggered && (stateMachine.MoveModule.IsGrounded || stateMachine.MoveModule.CanUseGroundJump))
            {
                stateMachine.InputReader.ConsumeJumpInput();

                stateMachine.MoveModule.TriggerJump(); // Gửi giấy nợ Nhảy cho Tài xế

                stateMachine.ChangeState(stateMachine.JumpState);
                return;
            }

            // Môi trường (Đang đứng yên mà đất dưới chân sập -> Rớt lầu)
            if (stateMachine.MoveModule.TimeSinceUngrounded > FallGraceTime && stateMachine.MoveModule.CurrentVelocityY < 0f)
            {
                stateMachine.ChangeState(stateMachine.FallState);
                return;
            }

            // QUYỀN LỰC 3: Muốn chạy
            if (Mathf.Abs(moveInput) >= MoveThreshold)
            {
                stateMachine.MoveModule.SetMoveInput(moveInput);
                stateMachine.ChangeState(stateMachine.RunState);
                return;
            }

            if (TryEnterAttackState())
                return;

        }

        private bool TryEnterAttackState()
        {
            if (!stateMachine.InputReader.AttackTriggered)
                return false;

            bool started = stateMachine.AttackModule.BeginAttack(
                stateMachine.BasicAttackData,
                stateMachine.MoveModule.IsFacingRight
            );

            if (!started)
                return false;

            stateMachine.InputReader.ConsumeAttackInput();

            stateMachine.ChangeState(
                stateMachine.AttackState
            );

            return true;
        }
    }
}