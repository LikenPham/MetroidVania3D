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
        [SerializeField] private string hitVfxPoolKey = "HitVFX";

        private void Awake()
        {
            hurtbox.HitReceived += HandleHitFeedback;
        }

        private void HandleHitFeedback(HitFeedback feedback)
        {
            // Xử lý VFX + Sound
        }
    }
}