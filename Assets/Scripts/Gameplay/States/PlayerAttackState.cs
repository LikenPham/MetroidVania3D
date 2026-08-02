using Core.DataModels;
using Gameplay.StateMachine;
using UnityEngine;

namespace Gameplay.States
{
    public class PlayerAttackState : PlayerState
    {
        private const float MoveThreshold = 0.1f;
        private const float FallGraceTime = 0.06f;
        private const float MinimumExitCheckTime = 0.05f;

        private int attackAnimationHash;
        private float stateTime;
        private bool hasValidAttack;

        public PlayerAttackState(PlayerStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            stateTime = 0f;

            AttackDataSO attackData = stateMachine.AttackModule.CurrentAttack;

            hasValidAttack = attackData != null && stateMachine.AttackModule.IsAttacking;

            if (!hasValidAttack)
            {
                Debug.LogError(
                    $"{stateMachine.name}: Vào AttackState " +
                    "nhưng AttackModule chưa có AttackData.",
                    stateMachine
                );

                return;
            }

            /*
             * AnimationKey là khóa dữ liệu.
             * Animator chỉ nhận int hash từ EntityAnimDataSO.
             */
            attackAnimationHash =
                stateMachine.AnimData.GetHash(
                    attackData.AnimationKey
                );

            if (attackData.LockHorizontalMovement)
            {
                stateMachine.MoveModule.StopImmediately();
                stateMachine.MoveModule.SetMoveInput(0f);
            }

            stateMachine.AnimModule.PlayAnimation(
                attackAnimationHash,
                attackData.CrossFadeDuration
            );
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (!hasValidAttack)
            {
                stateMachine.ChangeState(
                    stateMachine.IdleState
                );

                return;
            }

            stateTime += Time.deltaTime;

            AttackDataSO attackData =
                stateMachine.AttackModule.CurrentAttack;

            if (attackData == null)
            {
                stateMachine.ChangeState(
                    stateMachine.IdleState
                );

                return;
            }

            HandleMovement(attackData);

            stateMachine.AttackModule.TickAttack();

            if (ShouldEnterFallState())
            {
                stateMachine.ChangeState(
                    stateMachine.FallState
                );

                return;
            }

            if (stateTime < MinimumExitCheckTime)
                return;

            if (!stateMachine.AnimModule.IsAnimationFinished(
                    attackAnimationHash))
            {
                return;
            }

            FinishAttackState();
        }

        private void HandleMovement(
            AttackDataSO attackData)
        {
            if (attackData.LockHorizontalMovement)
            {
                stateMachine.MoveModule.SetMoveInput(0f);
                return;
            }

            float moveInput =
                stateMachine.InputReader.MoveDirection.x;

            stateMachine.MoveModule.SetMoveInput(moveInput);
        }

        private bool ShouldEnterFallState()
        {
            return
                stateMachine.MoveModule.TimeSinceUngrounded >
                    FallGraceTime &&
                stateMachine.MoveModule.CurrentVelocityY < 0f;
        }

        private void FinishAttackState()
        {
            float moveInput =
                stateMachine.InputReader.MoveDirection.x;

            if (Mathf.Abs(moveInput) >= MoveThreshold)
            {
                stateMachine.MoveModule.SetMoveInput(moveInput);

                stateMachine.ChangeState(
                    stateMachine.RunState
                );

                return;
            }

            stateMachine.ChangeState(
                stateMachine.IdleState
            );
        }

        public override void Exit()
        {
            base.Exit();

            stateMachine.AttackModule.EndAttack();
            hasValidAttack = false;
        }
    }
}