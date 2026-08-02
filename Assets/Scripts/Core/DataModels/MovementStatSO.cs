using UnityEngine;
using Core.Interfaces;

namespace Core.DataModels
{
    [CreateAssetMenu(fileName = "NewMovementStat", menuName = "Metroidvania/DataModels/Stats/Movement")]
    public class MovementStatSO : ScriptableObject, IMovementStats
    {
        [Header("Ground Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;
        public float MoveSpeed => moveSpeed;

        [Header("Air Movement")]
        [SerializeField, Min(0f)] private float jumpForce = 15f;
        public float JumpForce => jumpForce;

        [SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.8f;
        public float AirControlMultiplier => airControlMultiplier;

        [Header("Jump Assist")]
        [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
        public float CoyoteTime => coyoteTime;

        [SerializeField, Min(0f)] private float jumpBufferTime = 0.12f;
        public float JumpBufferTime => jumpBufferTime;

        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.2f;
        public float JumpCutMultiplier => jumpCutMultiplier;

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            jumpForce = Mathf.Max(0f, jumpForce);
            airControlMultiplier = Mathf.Clamp01(airControlMultiplier);

            coyoteTime = Mathf.Max(0f, coyoteTime);
            jumpBufferTime = Mathf.Max(0f, jumpBufferTime);
            jumpCutMultiplier = Mathf.Clamp01(jumpCutMultiplier);
        }
    }
}