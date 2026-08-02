using Core.DataModels;
using System;

namespace Core.Interfaces
{
    public interface IMoveModule
    {
        bool IsFacingRight { get; }
        bool IsGrounded { get; }
        bool IsOnSlope { get; }

        bool HasBufferedJump { get; }
        bool CanUseGroundJump { get; }

        float TimeSinceUngrounded { get; }
        float CurrentVelocityY { get; }

        event Action JumpExecuted;

        void Setup(IMovementStats movementStats);

        void SetMoveInput(float directionX);
        void TriggerJump();
        void ApplyJumpCut();
        void StopImmediately();
        void ForceVelocityY(float forceY);
    }
}