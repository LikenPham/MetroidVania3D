using Core.DataModels;
using Gameplay.Combat;
using UnityEngine;

namespace Gameplay.Modules
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeleeHitbox))]
    public sealed class AttackModule : MonoBehaviour
    {
        [SerializeField] private MeleeHitbox meleeHitbox;

        public AttackDataSO CurrentAttack { get; private set; }

        public bool IsAttacking { get; private set; }
        public bool IsHitWindowOpen { get; private set; }

        private void Awake()
        {
            if (meleeHitbox == null)
            {
                meleeHitbox = GetComponent<MeleeHitbox>();
            }
        }

        /// <summary>
        /// Chuẩn bị một đòn đánh mới.
        /// Gọi trước khi chuyển vào PlayerAttackState.
        /// </summary>
        public bool BeginAttack(
            AttackDataSO attackData,
            bool isFacingRight)
        {
            if (attackData == null)
            {
                Debug.LogError(
                    $"{name}: Không thể bắt đầu AttackData null.",
                    this
                );

                return false;
            }

            if (IsAttacking)
                return false;

            CurrentAttack = attackData;
            IsAttacking = true;
            IsHitWindowOpen = false;

            meleeHitbox.BeginAttack(
                attackData,
                isFacingRight
            );

            return true;
        }

        /// <summary>
        /// Được PlayerAttackState gọi mỗi frame.
        /// Chỉ quét khi hit window đang mở.
        /// </summary>
        public void TickAttack()
        {
            if (!IsAttacking || !IsHitWindowOpen)
                return;

            meleeHitbox.Scan();
        }

        /// <summary>
        /// Gọi từ Animation Event tại frame bắt đầu có sát thương.
        /// </summary>
        public void OpenHitWindow()
        {
            if (!IsAttacking || IsHitWindowOpen)
                return;

            IsHitWindowOpen = true;

            meleeHitbox.BeginHitWindow();

            /*
             * Quét ngay tại frame mở hit window.
             * Các frame tiếp theo do TickAttack xử lý.
             */
            meleeHitbox.Scan();
        }

        /// <summary>
        /// Gọi từ Animation Event tại frame kết thúc sát thương.
        /// </summary>
        public void CloseHitWindow()
        {
            if (!IsAttacking)
                return;

            IsHitWindowOpen = false;
        }

        public void EndAttack()
        {
            IsHitWindowOpen = false;
            IsAttacking = false;
            CurrentAttack = null;

            meleeHitbox.EndAttack();
        }
    }
}