using Core.DataModels;
using Core.Interfaces;
using UnityEngine;
using System;

namespace Gameplay.Modules
{
    // Ép Unity tự động gắn Rigidbody khi ném script này vào nhân vật (Tránh Game Designer quên)
    [RequireComponent(typeof(CharacterSensor))]
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMoveModule : MonoBehaviour, IMoveModule
    {
        [Header("Visual")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float modelRightFacingAngle = 90f;

        // Biến lưu trữ dữ liệu được tiêm vào
        private Rigidbody rb;
        private IMovementStats activeStats;

        private ISurfaceSensor surfaceSensor;
        private IWallSensor wallSensor;

        private float desiredDirectionX;

        private float coyoteTimer;
        private float jumpBufferTimer;

        private bool wasGroundedLastFixedFrame;
        private bool hasConsumedGroundJump;

        private bool isSetupComplete = false; // Cờ an toàn tuyệt đối

        // Thuộc tính công khai (chỉ đọc) cho StateMachine hỏi vòng vòng
        public bool IsFacingRight { get; private set; } = true;
        public bool IsGrounded => surfaceSensor != null && surfaceSensor.IsGrounded;
        public bool IsOnSlope => surfaceSensor != null && surfaceSensor.IsOnSlope;
        public float TimeSinceUngrounded { get; private set; }
        public float CurrentVelocityY => rb.linearVelocity.y;

        public bool HasBufferedJump => jumpBufferTimer > 0f;
        public bool CanUseGroundJump => coyoteTimer > 0f && !hasConsumedGroundJump;

        public event Action JumpExecuted;


        private void Awake()
        {
            // Quy tắc 4: Caching component 1 lần duy nhất
            rb = GetComponent<Rigidbody>();
            CharacterSensor characterSensor = GetComponent<CharacterSensor>();
            surfaceSensor = characterSensor;
            wallSensor = characterSensor;

            // Khóa trục Z và khóa xoay bằng code để chắc chắn Designer không quên tick ngoài Inspector
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = true;
        }

        // ==========================================
        // HÀM BƠM DỮ LIỆU (DEPENDENCY INJECTION)
        // ==========================================

        // Não bộ (PlayerController) sẽ gọi hàm này từ Awake/Start của nó
        public void Setup(IMovementStats movementStats)
        {
            activeStats = movementStats;
            isSetupComplete = activeStats != null;
        }

        private void FixedUpdate()
        {
            if (!isSetupComplete || activeStats == null) return;
            if (surfaceSensor == null || wallSensor == null) return;

            UpdateCoyoteTimer();
            UpdateJumpBufferTimer();

            ExecuteHorizontalMovement();
            TryConsumeBufferedJump();
        }

        // ==========================================
        // CÁC HÀM API DÀNH CHO STATEMACHINE GỌI
        // ==========================================

        public void SetMoveInput(float directionX)
        {
            desiredDirectionX = Mathf.Clamp(directionX, -1f, 1f);
            HandleFacing(directionX);
        }

        public void TriggerJump()
        {
            if (!isSetupComplete || activeStats == null)
                return;

            jumpBufferTimer = activeStats.JumpBufferTime;
        }

        public void StopImmediately()
        {
            desiredDirectionX = 0f;
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        public void ApplyJumpCut()
        {
            if (!isSetupComplete || activeStats == null)
                return;

            if (rb.linearVelocity.y <= 0f)
                return;

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                rb.linearVelocity.y * activeStats.JumpCutMultiplier,
                0f
            );
        }

        // =========================================================
        // LOGIC THỰC THI TRONG LUỒNG FIXEDUPDATE()
        // =========================================================

        private void UpdateCoyoteTimer()
        {
            bool isGrounded = IsGrounded;

            if (isGrounded)
            {
                TimeSinceUngrounded = 0f;

                // Khi vừa chạm đất lại thì reset quyền nhảy ground.
                if (!wasGroundedLastFixedFrame)
                {
                    hasConsumedGroundJump = false;
                }

                // Chỉ cấp lại coyote nếu không đang bay lên.
                // Cái này tránh case vừa ExecuteJump xong nhưng sensor vẫn grounded 1 frame,
                // rồi bị cấp lại coyote sai.
                if (rb.linearVelocity.y <= 0.05f)
                {
                    coyoteTimer = activeStats.CoyoteTime;
                }
            }
            else
            {
                TimeSinceUngrounded += Time.fixedDeltaTime;
                coyoteTimer -= Time.fixedDeltaTime;
            }

            wasGroundedLastFixedFrame = isGrounded;
        }

        private void UpdateJumpBufferTimer()
        {
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryConsumeBufferedJump()
        {
            if (jumpBufferTimer <= 0f)
                return;

            if (!CanUseGroundJump)
                return;

            ExecuteJump();
        }

        private void ExecuteJump()
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            hasConsumedGroundJump = true;

            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x,
                activeStats.JumpForce,
                0f
            );

            JumpExecuted?.Invoke();
        }

        public void ForceVelocityY(float forceY)
        {
            // Cưỡng chế ghi đè vận tốc trục Y ngay lập tức
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, forceY, 0);
        }

        // =========================================================
        // HORIZONTAL MOVEMENT
        // =========================================================

        private void ExecuteHorizontalMovement()
        {
            float controlMultiplier = IsGrounded
                ? 1f
                : activeStats.AirControlMultiplier;
            float targetScalarSpeed = desiredDirectionX * activeStats.MoveSpeed * controlMultiplier;
            Vector3 desiredVelocity = new Vector3(targetScalarSpeed, rb.linearVelocity.y, 0f);

            // =====================================================================
            // KỶ LUẬT TRƯỢT TƯỜNG TOÁN HỌC (Wall Projection) - Trị Tội ác số 2
            // =====================================================================
            if (wallSensor.IsTouchingRightWall && desiredDirectionX > 0)
            {
                desiredVelocity.x = 0f;
            }
            else if (wallSensor.IsTouchingLeftWall && desiredDirectionX < 0)
            {
                desiredVelocity.x = 0f;
            }
            else if (IsGrounded && IsOnSlope)
            {
                // Xử lý đi dốc cầu thang...
                Vector3 flatMove = new Vector3(desiredDirectionX, 0f, 0f).normalized;

                Vector3 slopeDirection = Vector3.ProjectOnPlane(flatMove, surfaceSensor.SurfaceNormal).normalized;

                desiredVelocity = slopeDirection *
                                  (Mathf.Abs(desiredDirectionX) * activeStats.MoveSpeed);
            }

            rb.linearVelocity = desiredVelocity;
        }


        // =========================================================
        // FACING
        // =========================================================

        private void HandleFacing(float directionX)
        {
            if (Mathf.Approximately(directionX, 0f))
                return;

            bool isMovingRight = directionX > 0f;

            if (isMovingRight == IsFacingRight)
                return;

            IsFacingRight = isMovingRight;

            if (visualRoot != null)
            {
                float targetAngleY = IsFacingRight
                    ? modelRightFacingAngle
                    : modelRightFacingAngle + 180f;

                visualRoot.localRotation = Quaternion.Euler(0f, targetAngleY, 0f);
            }
        }
    }
}