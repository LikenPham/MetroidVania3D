using Gameplay.Modules;
using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class IdleState : IEnemyState
    {
        private readonly AnimationModule animation;
        private readonly int idleAnimationHash;
        private readonly float idleDuration;

        private float idleTimer;

        public bool IsFinished => idleTimer >= idleDuration;

        public IdleState(
            AnimationModule animation,
            int idleAnimationHash,
            float idleDuration)
        {
            this.animation = animation;
            this.idleAnimationHash = idleAnimationHash;
            this.idleDuration = idleDuration;
        }

        public void Enter()
        {
            idleTimer = 0f;

            animation.PlayAnimation(idleAnimationHash);
        }

        public void Tick()
        {
            idleTimer += Time.deltaTime;
        }

        public void Exit()
        {
        }
    }
}