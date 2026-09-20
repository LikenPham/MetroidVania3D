using Core.DataModels;
using Core.Interfaces;
using Gameplay.Combat;
using UnityEngine;

namespace Gameplay.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class DamageTrap : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private int damageAmount = 10;

        private Collider trapCollider;

        private void Awake()
        {
            trapCollider = GetComponent<Collider>();
            trapCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Hurtbox hurtbox = other.GetComponent<Hurtbox>();

            if (hurtbox == null)
            {
                Debug.LogWarning(
                    $"{name}: {other.name} không tìm thấy Hurtbox.",
                    this
                );

                return;
            }

            DamageInfo damageInfo = new DamageInfo(damageAmount, transform.position);

            bool damaged = hurtbox.TryReceiveHit(damageInfo);

            if (!damaged)
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 direction = (hitPoint - transform.position).normalized;

            HitFeedback feedback = new HitFeedback(
                hitPoint,
                direction
            );

            hurtbox.SendHitFeedback(feedback);
        }
    }
}