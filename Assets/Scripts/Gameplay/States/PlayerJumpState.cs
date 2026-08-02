using Core.DataModels;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerJumpState : PlayerState
    {
        private const float MinimumStateTime = 0.05f;

        private readonly int startHash;
        private readonly int loopHash;

        private bool isLooping;
        private bool isJumpCutApplied;
        private float stateTime;

        public PlayerJumpState(PlayerStateMachine stateMachine) : base(stateMachine) 
        {
            startHash = stateMachine.AnimData.GetHash(AnimActionKey.Land);
            loopHash = stateMachine.AnimData.GetHash(AnimActionKey.Air);
        }

        public override void Enter()
        {
            base.Enter();
            stateTime = 0f;
            isLooping = false;
            isJumpCutApplied = false;

            stateMachine.AnimModule.PlayAnimation(startHash, 0f);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            stateTime += Time.deltaTime;

            HandleAirMovement();
            HandleJumpCut();

            if (TryEnterFallState())
                return;

            HandleAnimation();
        }

        private void HandleAirMovement()
        {
            float moveInput =
                stateMachine.InputReader.MoveDirection.x;

            stateMachine.MoveModule.SetMoveInput(moveInput);
        }

        private void HandleJumpCut()
        {
            if (isJumpCutApplied)
                return;

            if (stateMachine.InputReader.JumpPressed)
                return;

            if (stateMachine.MoveModule.CurrentVelocityY <= 0f)
                return;

            isJumpCutApplied = true;
            stateMachine.MoveModule.ApplyJumpCut();
        }

        private bool TryEnterFallState()
        {
            if (stateTime <= MinimumStateTime)
                return false;

            if (stateMachine.MoveModule.CurrentVelocityY >= 0f)
                return false;

            stateMachine.ChangeState(stateMachine.FallState);
            return true;
        }

        private void HandleAnimation()
        {
            if (isLooping || stateTime <= 0.05f)
                return;

            if (!stateMachine.AnimModule.IsAnimationFinished(startHash))
                return;

            isLooping = true;
            stateMachine.AnimModule.PlayAnimation(loopHash);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}