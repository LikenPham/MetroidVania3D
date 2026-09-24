using Core.DataModels;
using Gameplay.Modules;
using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class ChaseState : IEnemyState
    {
        private readonly MoveCommand moveLeftCommand;
        private readonly MoveCommand moveRightCommand;

        private readonly Transform enemyTransform;
        private readonly PlayerAnchorSO playerAnchor;
        private readonly EnemyWallSensor wallSensor;

        private readonly AnimationModule animation;
        private readonly int runAnimationHash;

        public bool IsBlockedByWall { get; private set; }

        private readonly float attackRangeSqr;

        public bool IsInAttackRange { get; private set; }

        public ChaseState(
            Transform enemyTransform,
            EnemyMovementModule movement,
            AnimationModule animation,
            int runAnimationHash,
            PlayerAnchorSO playerAnchor,
            float attackRange,
            EnemyWallSensor wallSensor)
        {
            this.enemyTransform = enemyTransform;
            this.playerAnchor = playerAnchor;

            this.animation = animation;
            this.runAnimationHash = runAnimationHash;

            this.wallSensor = wallSensor;

            attackRangeSqr = attackRange * attackRange;

            moveLeftCommand = new MoveCommand(
                movement,
                Vector3.left
            );

            moveRightCommand = new MoveCommand(
                movement,
                Vector3.right
            );
        }

        public void Enter()
        {
            IsInAttackRange = false;
            IsBlockedByWall = false;

            animation.PlayAnimation(runAnimationHash);
        }

        public void Tick()
        {
            if (playerAnchor == null ||
                playerAnchor.PlayerTransform == null)
            {
                IsInAttackRange = false;
                IsBlockedByWall = false;
                return;
            }

            Vector3 offset =
                playerAnchor.PlayerTransform.position -
                enemyTransform.position;

            IsInAttackRange =
                offset.sqrMagnitude <= attackRangeSqr;

            if (IsInAttackRange)
            {
                IsBlockedByWall = false;
                return;
            }

            bool isFacingRight = offset.x > 0f;

            wallSensor.Check(isFacingRight);

            if (wallSensor.IsWallAhead)
            {
                IsBlockedByWall = true;
                return;
            }

            IEnemyCommand command;

            if (isFacingRight)
            {
                command = moveRightCommand;
            }
            else
            {
                command = moveLeftCommand;
            }

            command.Execute();

            animation.PlayAnimation(runAnimationHash);
        }

        public void Exit()
        {
            IsBlockedByWall = false;
        }
    }
}