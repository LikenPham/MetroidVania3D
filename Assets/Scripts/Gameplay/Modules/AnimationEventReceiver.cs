using UnityEngine;
// Tương lai sẽ dùng namespace này để gọi âm thanh
// using Core.Events; 

namespace Gameplay.Modules
{
    // BẮT BUỘC: Script này phải được gắn lên CÙNG GameObject đang chứa Animator của con Armature
    [RequireComponent(typeof(Animator))]
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private AttackModule attackModule;

        private void Awake()
        {
            if (attackModule == null)
            {
                attackModule = GetComponent<AttackModule>();
            }
        }

        // 1. Tên hàm phải TRÙNG KHỚP 100% với tên báo lỗi trên Console
        // Tham số truyền vào có thể là AnimationEvent (chứa dữ liệu từ file gốc)
        public void OnFootstep(AnimationEvent animationEvent)
        {
            // Hiện tại: Bỏ trống hàm này để "nuốt" sự kiện, triệt tiêu lỗi Console.
            // Tương lai: Phát âm thanh bước chân thông qua EventManager
            // EventManager.RaisePlayAudio(FootstepSound);
        }

        // 2. Phòng hờ: Con Armature của Starter Assets thường có thêm sự kiện chạm đất
        public void OnLand(AnimationEvent animationEvent)
        {
            // Tương tự, bắt sóng và nuốt lỗi.
        }

        public void OpenAttackHitWindow()
        {
            attackModule.OpenHitWindow();
        }

        public void CloseAttackHitWindow()
        {
            attackModule.CloseHitWindow();
        }
    }
}