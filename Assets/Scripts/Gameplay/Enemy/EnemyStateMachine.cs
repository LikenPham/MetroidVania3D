using Core.DataModels;
using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class EnemyStateMachine
    {
        [SerializeField]
        private EntityAnimDataSO animationData;

        private IEnemyState currentState;

        public IEnemyState CurrentState => currentState;

        public void ChangeState(IEnemyState newState)
        {
            if (newState == null)
                return;

            if (currentState == newState)
                return;

            currentState?.Exit();

            currentState = newState;

            currentState.Enter();
        }

        public void Tick()
        {
            currentState?.Tick();
        }
    }
}