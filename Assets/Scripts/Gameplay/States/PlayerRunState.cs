using Core.DataModels;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerRunState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float FallGraceTime = 0.06f;

        private int runHash;
        public PlayerRunState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            runHash = stateMachine.AnimData.GetHash(AnimActionKey.Run);
        }

        public override void Enter()
        {
            // VD: Gọi Animator bật animation chạy (StateMachine sẽ giữ tham chiếu Animator)
            stateMachine.AnimModule.PlayAnimation(runHash);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            // 1. Đọc giác quan
            float moveInput = stateMachine.InputReader.MoveDirection.x;

            stateMachine.MoveModule.SetMoveInput(moveInput);

            if (stateMachine.InputReader.JumpTriggered && (stateMachine.MoveModule.IsGrounded || stateMachine.MoveModule.CanUseGroundJump))
            {
                stateMachine.InputReader.ConsumeJumpInput();

                // Gửi thư cho Cơ bắp: "Lát nữa FixedUpdate nhớ nhảy nhé!"
                stateMachine.MoveModule.TriggerJump();

                stateMachine.ChangeState(stateMachine.JumpState);
                return;
            }

            if (stateMachine.MoveModule.TimeSinceUngrounded > FallGraceTime && stateMachine.MoveModule.CurrentVelocityY < 0f)
            {
                stateMachine.ChangeState(stateMachine.FallState);
                return;
            }

            if (Mathf.Abs(moveInput) < MoveThreshold)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
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

        /*
        public override void PhysicsUpdate()
        {
            
        }
        */
    }
}