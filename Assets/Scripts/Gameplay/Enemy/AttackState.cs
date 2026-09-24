using Core.DataModels;
using Gameplay.Combat;
using Gameplay.Modules;

namespace Gameplay.Enemy
{
    public sealed class AttackState : IEnemyState
    {
        private readonly AnimationModule animation;
        private readonly int attackAnimationHash;
        private readonly MeleeHitbox meleeHitbox;
        private readonly AttackDataSO attackData;
        private readonly EnemyMovementModule movement;

        private bool attackStarted;

        public bool IsFinished { get; private set; }

        public AttackState(
            AnimationModule animation,
            EnemyMovementModule movement,
            MeleeHitbox meleeHitbox,
            AttackDataSO attackData,
            int attackAnimationHash)
        {
            this.animation = animation;
            this.movement = movement;
            this.meleeHitbox = meleeHitbox;
            this.attackData = attackData;
            this.attackAnimationHash = attackAnimationHash;
        }

        public void Enter()
        {
            IsFinished = false;
            attackStarted = false;

            meleeHitbox.BeginAttack(
                attackData,
                movement.IsFacingRight
            );

            animation.PlayAnimation(attackAnimationHash);

            attackStarted = true;
        }

        public void Tick()
        {
            if (!attackStarted)
                return;

            if (!IsFinished)
            {
                meleeHitbox.Scan();
            }

            if (animation.IsAnimationFinished(attackAnimationHash))
            {
                IsFinished = true;
            }
        }


        public void Exit()
        {
            meleeHitbox.EndAttack();
        }
    }
}