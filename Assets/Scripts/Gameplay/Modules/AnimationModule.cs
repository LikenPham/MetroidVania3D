using UnityEngine;

namespace Gameplay.Modules
{
    public class AnimationModule : MonoBehaviour
    {
        // Tham chiếu đến thành phần đồ họa của Unity
        private Animator animator;

        private void Awake()
        {
            // Caching
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        // ==========================================
        // API CHO STATE MACHINE GỌI
        // ==========================================
        public void PlayAnimation(int animationHash, float transitionDuration = 0.05f)
        {
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isTransitioning = animator.IsInTransition(0);

            // LUẬT 1: Chặn lệnh thừa khi ĐANG CHUẨN BỊ tới đích
            if (isTransitioning && animator.GetNextAnimatorStateInfo(0).shortNameHash == animationHash)
            {
                return;
            }

            // LUẬT 2: Chặn lệnh thừa khi ĐANG Ở SẴN đích an toàn (LƯU Ý: Phải thêm !isTransitioning)
            // Chỉ bỏ qua lệnh nếu đĩa đang chạy là đĩa mình muốn, VÀ hệ thống KHÔNG đang trong quá trình chuyển sang đĩa khác.
            if (!isTransitioning && currentInfo.shortNameHash == animationHash)
            {
                return;
            }

            // Chạy animation dựa trên mã băm đã được truyền vào
            animator.CrossFadeInFixedTime(animationHash, transitionDuration); // 0f vì game 2D thường không cần pha trộn (blend) chuyển động như 3D
        }

        // CẢM BIẾN BÁO CÁO TIẾN ĐỘ HOẠT ẢNH
        public bool IsAnimationFinished(int animationHash)
        {
            // Lấy thông tin cái đĩa đang chạy hiện tại ở Layer 0
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 1. Kiểm tra xem đĩa đang chạy có ĐÚNG là cái đĩa mình đang hỏi không?
            // 2. Độ dài đã chạy (normalizedTime) đạt 95% chưa? (Dùng 0.95 thay vì 1.0 để chống lỗi lệch Frame của Unity)
            // 3. Đảm bảo nó không bị kẹt trong lúc đang chuyển cảnh (CrossFade)
            if (stateInfo.shortNameHash == animationHash)
            {
                return stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0);
            }

            return false;
        }
    }
}