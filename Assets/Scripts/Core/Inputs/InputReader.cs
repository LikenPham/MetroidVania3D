using System;
using UnityEngine;
using UnityEngine.InputSystem; // Thư viện bắt buộc của New Input System

namespace Core.Input
{
    // Cần phải tick chọn "Generate C# Class" trong file Input Action Asset (ví dụ tên là GameInput)
    // Sau đó cho InputReader kế thừa interface IPlayerActions do file đó sinh ra.
    public class InputReader : MonoBehaviour, GameInput.IPlayerActions
    {
        public Vector2 MoveDirection { get; private set; } // Khóa set lại, chỉ cho bên ngoài đọc (get)

        public bool JumpTriggered { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpReleased { get; private set; }

        public bool AttackTriggered { get; private set; }

        public event Action OnJumpEvent;
        public event Action OnAttackEvent;
        public event Action OnInteractEvent;

        // Biến lưu trữ cỗ máy Input
        private GameInput gameInput;

        private void OnEnable()
        {
            if (gameInput == null)
            {
                gameInput = new GameInput();
                // Báo cho GameInput biết: "Tao (this) sẽ nhận trách nhiệm xử lý các nút bấm của Player"
                gameInput.Player.SetCallbacks(this);
            }
            gameInput.Player.Enable(); // Bật bộ đọc phím
        }

        private void OnDisable()
        {
            gameInput.Player.Disable(); // Tắt bộ đọc phím khi nhân vật chết hoặc bị hủy

            MoveDirection = Vector2.zero;
            JumpTriggered = false;
            JumpPressed = false;
            JumpReleased = false;

            AttackTriggered = false;
        }

        // =========================================================
        // THỰC THI INTERFACE (Tốc độ ánh sáng, không dùng Reflection)
        // =========================================================

        public void OnMove(InputAction.CallbackContext context)
        {
            // Liên tục cập nhật Vector2 mỗi khi cần gạt di chuyển
            MoveDirection = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // context.performed đảm bảo sự kiện chỉ phát ra 1 lần khi nút được bấm XUỐNG
            if (context.performed)
            {
                JumpTriggered = true;
                JumpPressed = true;
                JumpReleased = false;
                OnJumpEvent?.Invoke();
            }
            if (context.canceled)
            {
                JumpPressed = false;
                JumpReleased = true;
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                AttackTriggered = true;
                OnAttackEvent?.Invoke();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnInteractEvent?.Invoke();
            }
        }

        public void ConsumeJumpInput()
        {
            // HẠ BIỂN BÁO XUỐNG: Để chống lỗi nhảy đúp (Double Jump)
            JumpTriggered = false;
        }
        public void ConsumeJumpReleaseInput()
        {
            JumpReleased = false;
        }

        public void ConsumeAllJumpInput()
        {
            JumpTriggered = false;
            JumpReleased = false;
        }

        public void ConsumeAttackInput()
        {
            AttackTriggered = false;
        }
    }
}