using Core.DataModels;
using Gameplay.Combat;
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
        [SerializeField] private EnemyWallSensor wallSensor;

        [Header("Patrol")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float idleDuration = 1f;

        [Header("Detection")]
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private LayerMask playerLayer;

        private readonly Collider[] detectionResults = new Collider[4];

        [SerializeField] private PlayerAnchorSO playerAnchor;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private MeleeHitbox meleeHitbox;
        [SerializeField] private AttackDataSO attackData;

        private EnemyStateMachine stateMachine;
        private PatrolState patrolState;

        private IdleState idleState;
        private int idleAnimationHash;

        private ChaseState chaseState;
        private AttackState attackState;

        private void Awake()
        {
            animationData.InitializeHashes();

            int runAnimationHash =
                animationData.GetHash(AnimActionKey.Run);

            idleAnimationHash =
                animationData.GetHash(AnimActionKey.Idle);

            stateMachine = new EnemyStateMachine();

            patrolState = new PatrolState(
                transform,
                movement,
                animationModule,
                runAnimationHash,
                pointA,
                pointB
            );

            chaseState = new ChaseState(
                transform,
                movement,
                animationModule,
                runAnimationHash,
                playerAnchor,
                attackRange,
                wallSensor
            );

            int attackAnimationHash =
                animationData.GetHash(AnimActionKey.Attack);

            attackState = new AttackState(
                animationModule,
                movement,
                meleeHitbox,
                attackData,
                attackAnimationHash
            );

            idleState = new IdleState(
                animationModule,
                idleAnimationHash,
                idleDuration
            );
        }

        private void Start()
        {
            stateMachine.ChangeState(patrolState);
        }

        private void Update()
        {
            stateMachine.Tick();

            if (stateMachine.CurrentState == patrolState)
            {
                if (CanDetectPlayer())
                {
                    stateMachine.ChangeState(chaseState);
                    return;
                }

                if (patrolState.HasReachedPoint)
                {
                    stateMachine.ChangeState(idleState);
                    return;
                }
            }
            else if (stateMachine.CurrentState == idleState)
            {
                if (CanDetectPlayer())
                {
                    stateMachine.ChangeState(chaseState);
                    return;
                }

                if (idleState.IsFinished)
                {
                    stateMachine.ChangeState(patrolState);
                    return;
                }
            }
            else if (stateMachine.CurrentState == chaseState)
            {
                if (chaseState.IsInAttackRange)
                {
                    stateMachine.ChangeState(attackState);
                    return;
                }

                if (chaseState.IsBlockedByWall)
                {
                    stateMachine.ChangeState(patrolState);
                    return;
                }
            }
            else if (stateMachine.CurrentState == attackState)
            {
                if (attackState.IsFinished)
                {
                    stateMachine.ChangeState(chaseState);
                    return;
                }
            }
        }

        private bool CanDetectPlayer()
        {
            return Physics.OverlapSphereNonAlloc(
                transform.position,
                detectionRange,
                detectionResults,
                playerLayer
            ) > 0;
        }
    }
}