using Core.DataModels;
using Core.Input;
using Core.Interfaces;
using Gameplay.Modules;
using Gameplay.States;
using UnityEngine;

namespace Gameplay.StateMachine
{
    public class PlayerStateMachine : MonoBehaviour
    {
        [Header("Giác Quan & Dữ Liệu")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private MovementStatSO movementStats; // Sẽ dùng để bơm dữ liệu sau
        [SerializeField] private EntityAnimDataSO animData;
        [SerializeField] private MonoBehaviour moveModuleBehaviour;
        [SerializeField] private AttackDataSO basicAttackData;

        // ==========================================
        // CÁC MODULE (TAY CHÂN) - Getter public để các State dễ dàng gọi tới
        // ==========================================
        public IMoveModule MoveModule { get; private set; }
        public HealthModule HealthModule { get; private set; }
        public AnimationModule AnimModule { get; private set; }
        public AttackModule AttackModule { get; private set; }
        public InputReader InputReader => inputReader;
        public EntityAnimDataSO AnimData => animData;
        public AttackDataSO BasicAttackData => basicAttackData;

        // ==========================================
        // DANH SÁCH TRẠNG THÁI CACHE SẴN (CHỐNG RÁC RAM)
        // ==========================================
        public PlayerIdleState IdleState { get; private set; }
        public PlayerRunState RunState { get; private set; }
        public PlayerJumpState JumpState { get; private set; }
        public PlayerFallState FallState { get; private set; }
        public PlayerLandState LandState { get; private set; }
        public PlayerAttackState AttackState { get; private set; }

        public PlayerState CurrentState { get; private set; }

        private void Awake()
        {
            // 1. Caching Modules (Quy tắc 4)
            MoveModule = moveModuleBehaviour as IMoveModule;

            if (MoveModule == null)
            {
                Debug.LogError($"{name}: moveModuleBehaviour must implement IMoveModule.");
                enabled = false;
                return;
            }

            MoveModule.Setup(movementStats);

            HealthModule = GetComponent<HealthModule>();
            AnimModule = GetComponent<AnimationModule>();
            AttackModule = GetComponent<AttackModule>();

            animData.InitializeHashes();

            // 2. Khởi tạo State 1 lần duy nhất và nhét "this" (chính StateMachine này) vào
            // Lát nữa code file State xong chúng ta sẽ mở comment ra
            IdleState = new PlayerIdleState(this);
            RunState = new PlayerRunState(this);
            JumpState = new PlayerJumpState(this);
            FallState = new PlayerFallState(this);
            LandState = new PlayerLandState(this);
            AttackState = new PlayerAttackState(this);

            if (AttackModule == null)
            {
                Debug.LogError(
                    $"{name}: Thiếu AttackModule.",
                    this
                );

                enabled = false;
                return;
            }
        }

        private void Start()
        {
            // Bắt đầu nhịp tim bằng trạng thái mặc định
            Initialize(IdleState);
        }

        // ==========================================
        // CƠ CHẾ VẬN HÀNH (CORE LOOP)
        // ==========================================

        public void Initialize(PlayerState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        public void ChangeState(PlayerState newState)
        {
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        private void Update()
        {
            // Chỉ ủy quyền cho Trạng thái hiện tại xử lý logic, Không tự nghe Input!
            if (CurrentState != null)
            {
                CurrentState.LogicUpdate();
            }
        }

        private void FixedUpdate()
        {
            // Ủy quyền xử lý vật lý
            if (CurrentState != null)
            {
                CurrentState.PhysicsUpdate();
            }
        }
    }
}