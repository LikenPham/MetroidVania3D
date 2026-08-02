using Core.Interfaces;
using UnityEngine;

namespace Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class Hurtbox : MonoBehaviour
    {
        [Header("Damage Receiver")]
        [Tooltip(
            "Kéo component có implement IDamageable vào đây, " +
            "thường là HealthModule trên Enemy Root."
        )]
        [SerializeField] private MonoBehaviour damageReceiverBehaviour;

        private IDamageable damageReceiver;

        public IDamageable DamageReceiver => damageReceiver;

        public bool IsReady => damageReceiver != null;

        private void Awake()
        {
            CacheDamageReceiver();
        }

        private void CacheDamageReceiver()
        {
            damageReceiver =
                damageReceiverBehaviour as IDamageable;

            if (damageReceiver != null)
                return;

            Debug.LogError(
                $"{name}: Damage Receiver phải implement IDamageable.",
                this
            );

            enabled = false;
        }

        /// <summary>
        /// Dùng khi một object động được Spawn và cần truyền receiver trực tiếp.
        /// </summary>
        public void Setup(IDamageable receiver)
        {
            damageReceiver = receiver;
            damageReceiverBehaviour = receiver as MonoBehaviour;

            enabled = damageReceiver != null;

            if (damageReceiver == null)
            {
                Debug.LogError(
                    $"{name}: Hurtbox.Setup nhận IDamageable null.",
                    this
                );
            }
        }

        public bool TryReceiveHit(DamageInfo damageInfo)
        {
            if (damageReceiver == null)
                return false;

            damageReceiver.TakeDamage(damageInfo);
            return true;
        }
    }
}