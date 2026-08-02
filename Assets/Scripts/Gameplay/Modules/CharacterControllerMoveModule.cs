using System;
using Core.Interfaces;
using UnityEngine;

namespace Gameplay.Modules
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(CharacterController))]
    public class CharacterControllerMoveModule : MonoBehaviour, IMoveModule, IGroundSensor
    {
        [Header("Visual")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float modelRightFacingAngle = 90f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -35f;
        [SerializeField] private float groundedStickVelocity = -2f;
        [SerializeField] private float terminalVelocity = -50f;

        [Header("2.5D Constraint")]
        [SerializeField] private bool lockZPosition = true;
        [SerializeField] private float lockedZ = 0f;

        private CharacterController controller;
        private IMovementStats activeStats;

        private ISurfaceSensor surfaceSensor;
        private IWallSensor wallSensor;

        private float desiredDirectionX;
        private float verticalVelocity;

        private float coyoteTimer;
        private float jumpBufferTimer;

        private bool wasGroundedLastFrame;
        private bool hasConsumedGroundJump;
        private bool isSetupComplete;

        public bool IsFacingRight { get; private set; } = true;

        // Với CharacterController, grounded chính nên lấy từ controller.
        // Sensor chỉ dùng phụ để lấy slope/wall info.
        public bool IsGrounded => controller != null && controller.isGrounded;

        public bool IsOnSlope => surfaceSensor != null && surfaceSensor.IsOnSlope;

        public float TimeSinceUngrounded { get; private set; }

        public float CurrentVelocityY => verticalVelocity;

        public bool HasBufferedJump => jumpBufferTimer > 0f;

        public bool CanUseGroundJump => coyoteTimer > 0f && !hasConsumedGroundJump;

        public event Action JumpExecuted;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            CharacterSensor characterSensor = GetComponent<CharacterSensor>();
            surfaceSensor = characterSensor;
            wallSensor = characterSensor;

            if (lockZPosition)
            {
                lockedZ = transform.position.z;
            }
        }

        public void Setup(IMovementStats movementStats)
        {
            activeStats = movementStats;
            isSetupComplete = activeStats != null;
        }

        private void Update()
        {
            if (!isSetupComplete || activeStats == null)
                return;

            UpdateCoyoteTimer();
            UpdateJumpBufferTimer();

            TryConsumeBufferedJump();

            ApplyGravity();

            ExecuteMovement();
        }

        // =========================================================
        // API CHO FSM / PLAYER CONTROLLER GỌI
        // =========================================================

        public void SetMoveInput(float directionX)
        {
            desiredDirectionX = Mathf.Clamp(directionX, -1f, 1f);
            HandleFacing(desiredDirectionX);
        }

        public void TriggerJump()
        {
            if (!isSetupComplete || activeStats == null)
                return;

            jumpBufferTimer = activeStats.JumpBufferTime;
        }

        public void ApplyJumpCut()
        {
            if (!isSetupComplete || activeStats == null)
                return;

            if (verticalVelocity <= 0f)
                return;

            verticalVelocity *= activeStats.JumpCutMultiplier;
        }

        public void StopImmediately()
        {
            desiredDirectionX = 0f;
        }

        public void ForceVelocityY(float forceY)
        {
            verticalVelocity = forceY;
        }

        // =========================================================
        // COYOTE TIME + JUMP BUFFER
        // =========================================================

        private void UpdateCoyoteTimer()
        {
            bool grounded = IsGrounded;

            if (grounded)
            {
                TimeSinceUngrounded = 0f;

                if (!wasGroundedLastFrame)
                {
                    hasConsumedGroundJump = false;
                }

                if (verticalVelocity <= 0f)
                {
                    coyoteTimer = activeStats.CoyoteTime;
                }
            }
            else
            {
                TimeSinceUngrounded += Time.deltaTime;
                coyoteTimer -= Time.deltaTime;
            }

            wasGroundedLastFrame = grounded;
        }

        private void UpdateJumpBufferTimer()
        {
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
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

            verticalVelocity = activeStats.JumpForce;

            JumpExecuted?.Invoke();
        }

        // =========================================================
        // GRAVITY + MOVEMENT
        // =========================================================

        private void ApplyGravity()
        {
            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
                return;
            }

            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }

        private void ExecuteMovement()
        {
            float controlMultiplier = IsGrounded
                ? 1f
                : activeStats.AirControlMultiplier;

            float moveX = desiredDirectionX * activeStats.MoveSpeed * controlMultiplier;

            if (wallSensor != null)
            {
                if (wallSensor.IsTouchingRightWall && desiredDirectionX > 0f)
                {
                    moveX = 0f;
                }
                else if (wallSensor.IsTouchingLeftWall && desiredDirectionX < 0f)
                {
                    moveX = 0f;
                }
            }

            Vector3 motion = new Vector3(
                moveX,
                verticalVelocity,
                0f
            );

            CollisionFlags flags = controller.Move(motion * Time.deltaTime);

            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }

            if (lockZPosition)
            {
                Vector3 position = transform.position;
                position.z = lockedZ;
                transform.position = position;
            }
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

            if (visualRoot == null)
                return;

            float targetAngleY = IsFacingRight
                ? modelRightFacingAngle
                : modelRightFacingAngle + 180f;

            visualRoot.localRotation = Quaternion.Euler(
                0f,
                targetAngleY,
                0f
            );
        }
    }
}