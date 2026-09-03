using Core.DataModels;
using Gameplay.Combat;
using UnityEngine;

namespace Core.FeedbackVFX
{
    [DisallowMultipleComponent]
    public sealed class HitVFXController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Hurtbox hurtbox;

        [Header("VFX")]
        [SerializeField] private string hitVfxPoolKey = "HitVFX";
    }
}