using Core.DataModels;
using Gameplay.Modules;
using UnityEngine;

namespace Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public class EnemyAIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyMovementModule movement;
        [SerializeField] private AnimationModule animationModule;
        [SerializeField] private EntityAnimDataSO animationData;

        [Header("Patrol")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;

        private EnemyStateMachine stateMachine;
        private PatrolState patrolState;

        private void Awake()
        {
            animationData.InitializeHashes();

            int runAnimationHash =
                animationData.GetHash(AnimActionKey.Run);

            stateMachine = new EnemyStateMachine();

            patrolState = new PatrolState(
                transform,
                movement,
                animationModule,
                runAnimationHash,
                pointA,
                pointB
            );
        }

        private void Start()
        {
            stateMachine.ChangeState(patrolState);
        }

        private void Update()
        {
            stateMachine.Tick();
        }
    }
}