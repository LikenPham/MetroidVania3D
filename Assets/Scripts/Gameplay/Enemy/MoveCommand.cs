using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class MoveCommand : IEnemyCommand
    {
        private readonly EnemyMovementModule movement;
        private readonly Vector3 direction;

        public MoveCommand(
            EnemyMovementModule movement,
            Vector3 direction)
        {
            this.movement = movement;
            this.direction = direction;
        }

        public void Execute()
        {
            movement.Move(direction);
        }
    }
}