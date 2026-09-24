using Core.DataModels;
using Core.Interfaces;
using UnityEngine;

namespace Gameplay.Combat
{
    public sealed class MeleeHitbox : MonoBehaviour
    {
        [Header("References")]
        [Tooltip(
            "Mốc tính vị trí hitbox. Nên là Player Root " +
            "hoặc một Attack Origin không bị animation làm lệch."
        )]
        [SerializeField] private Transform hitboxOrigin;

        [Tooltip("Vị trí nguồn gây sát thương, thường là Player Root.")]
        [SerializeField] private Transform damageSource;

        [Header("Detection")]
        [SerializeField] private LayerMask targetLayers;

        [Tooltip("Số Collider tối đa có thể tìm thấy trong một lần quét.")]
        [SerializeField, Min(1)] private int resultBufferSize = 16;

        [Header("Editor Preview")]
        [SerializeField] private AttackDataSO previewAttackData;
        [SerializeField] private bool previewFacingRight = true;

        private Collider[] hitResults;

        /*
         * Lưu receiver thay vì lưu Collider.
         *
         * Nếu một Enemy có nhiều Hurtbox Collider nhưng tất cả đều trỏ
         * về cùng HealthModule, Enemy đó vẫn chỉ nhận damage một lần.
         */
        private IDamageable[] alreadyHitReceivers;
        private int alreadyHitCount;

        private AttackDataSO activeAttackData;
        private bool activeFacingRight;

        private void Awake()
        {
            if (hitboxOrigin == null)
            {
                hitboxOrigin = transform;
            }

            if (damageSource == null)
            {
                damageSource = transform;
            }

            int bufferSize = Mathf.Max(1, resultBufferSize);

            hitResults = new Collider[bufferSize];
            alreadyHitReceivers = new IDamageable[bufferSize];
        }

        public void BeginAttack(
            AttackDataSO attackData,
            bool isFacingRight)
        {
            activeAttackData = attackData;
            activeFacingRight = isFacingRight;

            ClearAlreadyHitReceivers();
        }

        /// <summary>
        /// Gọi khi Animation Event mở hit window.
        /// </summary>
        public void BeginHitWindow()
        {
            ClearAlreadyHitReceivers();
        }

        /// <summary>
        /// Quét Hurtbox trong vùng của đòn đánh hiện tại.
        /// Có thể gọi mỗi frame trong hit window.
        /// </summary>
        public int Scan()
        {
            if (activeAttackData == null)
                return 0;

            CalculateWorldBox(
                activeAttackData,
                activeFacingRight,
                out Vector3 worldCenter,
                out Quaternion worldRotation
            );

            int resultCount = Physics.OverlapBoxNonAlloc(
                worldCenter,
                activeAttackData.HitboxHalfExtents,
                hitResults,
                worldRotation,
                targetLayers,
                QueryTriggerInteraction.Collide
            );

            int appliedHitCount = 0;

            for (int i = 0; i < resultCount; i++)
            {
                Collider hitCollider = hitResults[i];

                if (hitCollider == null)
                    continue;

                if (!hitCollider.TryGetComponent(
                        out Hurtbox hurtbox))
                {
                    continue;
                }

                if (!hurtbox.IsReady)
                    continue;

                IDamageable receiver = hurtbox.DamageReceiver;

                if (HasAlreadyHit(receiver))
                    continue;

                DamageInfo damageInfo = new DamageInfo(
                    activeAttackData.Damage,
                    damageSource.position
                );

                if (!hurtbox.TryReceiveHit(damageInfo))
                    continue;

                RememberReceiver(receiver);
                appliedHitCount++;
            }

            return appliedHitCount;
        }

        public void EndAttack()
        {
            activeAttackData = null;
            ClearAlreadyHitReceivers();
        }

        private bool HasAlreadyHit(IDamageable receiver)
        {
            for (int i = 0; i < alreadyHitCount; i++)
            {
                if (ReferenceEquals(
                        alreadyHitReceivers[i],
                        receiver))
                {
                    return true;
                }
            }

            return false;
        }

        private void RememberReceiver(IDamageable receiver)
        {
            if (alreadyHitCount >= alreadyHitReceivers.Length)
                return;

            alreadyHitReceivers[alreadyHitCount] = receiver;
            alreadyHitCount++;
        }

        private void ClearAlreadyHitReceivers()
        {
            for (int i = 0; i < alreadyHitCount; i++)
            {
                alreadyHitReceivers[i] = null;
            }

            alreadyHitCount = 0;
        }

        private void CalculateWorldBox(
            AttackDataSO attackData,
            bool isFacingRight,
            out Vector3 worldCenter,
            out Quaternion worldRotation)
        {
            Vector3 localOffset =
                attackData.HitboxOffset;

            Vector3 localEulerAngles =
                attackData.HitboxEulerAngles;

            if (!isFacingRight)
            {
                localOffset.x = -localOffset.x;

                /*
                 * Game di chuyển trong mặt phẳng X-Y,
                 * nên hitbox xoay quanh Z cần đảo dấu khi quay trái.
                 */
                localEulerAngles.z = -localEulerAngles.z;
            }

            worldCenter =
                hitboxOrigin.TransformPoint(localOffset);

            worldRotation =
                hitboxOrigin.rotation *
                Quaternion.Euler(localEulerAngles);
        }

        private void OnDrawGizmos()
        {
            AttackDataSO dataToDraw =
                Application.isPlaying && activeAttackData != null
                    ? activeAttackData
                    : previewAttackData;

            if (dataToDraw == null)
                return;

            Transform origin =
                hitboxOrigin != null
                    ? hitboxOrigin
                    : transform;

            Vector3 localOffset =
                dataToDraw.HitboxOffset;

            Vector3 localEulerAngles =
                dataToDraw.HitboxEulerAngles;

            bool isFacingRight =
                Application.isPlaying
                    ? activeFacingRight
                    : previewFacingRight;

            if (!isFacingRight)
            {
                localOffset.x = -localOffset.x;
                localEulerAngles.z = -localEulerAngles.z;
            }

            Vector3 worldCenter =
                origin.TransformPoint(localOffset);

            Quaternion worldRotation =
                origin.rotation *
                Quaternion.Euler(localEulerAngles);

            Matrix4x4 previousMatrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(
                worldCenter,
                worldRotation,
                Vector3.one
            );

            Gizmos.color =
                new Color(1f, 0f, 0f, 0.8f);

            Gizmos.DrawWireCube(
                Vector3.zero,
                dataToDraw.HitboxSize
            );

            Gizmos.matrix = previousMatrix;
        }
    }
}