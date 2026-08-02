using Core.DataModels;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerFallState : PlayerState
    {
        private readonly int airHash;

        private float maxPeakY;
        private const float LightLandingDistance = 1.2f;
        private const float FallStartVelocity = -1.5f;
        public PlayerFallState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
            airHash = stateMachine.AnimData.GetHash(AnimActionKey.Air);
        }

        public override void Enter()
        {
            base.Enter();
            maxPeakY = stateMachine.transform.position.y;

            stateMachine.AnimModule.PlayAnimation(airHash, 0.1f);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (stateMachine.transform.position.y > maxPeakY)
            {
                maxPeakY = stateMachine.transform.position.y;
            }

            float moveInput = stateMachine.InputReader.MoveDirection.x;
            stateMachine.MoveModule.SetMoveInput(moveInput);

            if (stateMachine.InputReader.JumpTriggered)
            {
                stateMachine.MoveModule.TriggerJump();
                stateMachine.InputReader.ConsumeJumpInput();
            }


            if (stateMachine.MoveModule.IsGrounded)
            {
                float fallDistance = maxPeakY - stateMachine.transform.position.y;

                if (fallDistance < 1.2f)
                {
                    if (Mathf.Abs(moveInput) >= 0.1f)
                        stateMachine.ChangeState(stateMachine.RunState);
                    else
                        stateMachine.ChangeState(stateMachine.IdleState);
                }
                else
                {
                    stateMachine.ChangeState(stateMachine.LandState);
                }

                return;
            }

            if (stateMachine.MoveModule.CurrentVelocityY > -0.1f &&
    stateMachine.MoveModule.CurrentVelocityY < 0.5f)
            {
                stateMachine.MoveModule.ForceVelocityY(FallStartVelocity);
            }
        }

        private void HandleJumpInput()
        {
            if (!stateMachine.InputReader.JumpPressed)
                return;

            stateMachine.MoveModule.TriggerJump();

            // Chỉ consume nếu InputReader của bạn dùng kiểu input latch.
            // Nếu JumpPressedThisFrame tự reset mỗi frame thì không cần dòng này.
            stateMachine.InputReader.ConsumeJumpInput();
        }

        private void HandleLanding(float moveInput)
        {
            float fallDistance = maxPeakY - stateMachine.transform.position.y;

            if (fallDistance < LightLandingDistance)
            {
                if (Mathf.Abs(moveInput) >= 0.1f)
                {
                    stateMachine.ChangeState(stateMachine.RunState);
                }
                else
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                }

                return;
            }

            stateMachine.ChangeState(stateMachine.LandState);
        }

        public override void Exit()
        {
            base.Exit();
            stateMachine.InputReader.ConsumeJumpInput();
        }
    }
}