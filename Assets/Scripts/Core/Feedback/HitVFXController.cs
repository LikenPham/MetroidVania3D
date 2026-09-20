using Core.DataModels;
using Gameplay.Combat;
using UnityEngine;

namespace Core.Feedback
{
    [DisallowMultipleComponent]
    public sealed class HitVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Hurtbox hurtbox;

        [Header("VFX")]
        [SerializeField] private GameObject hitVFXPrefab;

        [Header("Settings")]
        [SerializeField] private Transform vfxRoot;

        private void Awake()
        {
            if (hurtbox == null)
                hurtbox = GetComponent<Hurtbox>();

            if (hurtbox == null)
            {
                Debug.LogError(
                    $"{name}: HitVFXController cần Hurtbox.",
                    this
                );

                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (hurtbox != null)
                hurtbox.HitReceived += OnHitReceived;
        }

        private void OnDisable()
        {
            if (hurtbox != null)
                hurtbox.HitReceived -= OnHitReceived;
        }

        private void OnHitReceived(HitFeedback feedback)
        {
            if (hitVFXPrefab == null)
                return;

            Transform parent = vfxRoot != null ? vfxRoot : null;

            GameObject vfx = Instantiate(
                hitVFXPrefab,
                feedback.HitPoint,
                Quaternion.LookRotation(feedback.Direction),
                parent
            );

            Destroy(vfx, 2f);
        }
    }
}