using Gameplay.Modules;
using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class PatrolState : IEnemyState
    {
        private readonly MoveCommand moveLeftCommand;
        private readonly MoveCommand moveRightCommand;

        private readonly Transform enemyTransform;
        private readonly Transform pointA;
        private readonly Transform pointB;

        private bool movingRight;

        private readonly int runAnimationHash;
        private readonly AnimationModule animation;

        public bool IsMovingRight => movingRight;

        public PatrolState(
            Transform enemyTransform,
            EnemyMovementModule movement,
            AnimationModule animation,
            int runAnimationHash,
            Transform pointA,
            Transform pointB)
        {
            this.enemyTransform = enemyTransform;
            this.animation = animation;
            this.runAnimationHash = runAnimationHash;

            this.pointA = pointA;
            this.pointB = pointB;

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
            movingRight = true;

            animation.PlayAnimation(runAnimationHash);
        }

        public void Tick()
        {
            if (movingRight)
            {
                if (enemyTransform.position.x >= pointB.position.x)
                {
                    movingRight = false;
                }
            }
            else
            {
                if (enemyTransform.position.x <= pointA.position.x)
                {
                    movingRight = true;
                }
            }

            IEnemyCommand command = movingRight
                ? moveRightCommand
                : moveLeftCommand;

            command.Execute();
            animation.PlayAnimation(runAnimationHash);
        }

        public void Exit()
        {
        }
    }
}