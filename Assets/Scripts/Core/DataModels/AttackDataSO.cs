using UnityEngine;

namespace Core.DataModels
{
    [CreateAssetMenu(
        fileName = "AttackData",
        menuName = "Game/Combat/Attack Data"
    )]
    public sealed class AttackDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string attackName = "Attack";

        [Tooltip("Tên khóa được khai báo trong EntityAnimDataSO.")]
        [SerializeField] private AnimActionKey animationKey = AnimActionKey.Attack;

        [Header("Damage")]
        [SerializeField, Min(0)] private int damage = 15;

        [Header("Hitbox")]
        [Tooltip("Vị trí hitbox tương đối so với Hitbox Origin.")]
        [SerializeField]
        private Vector3 hitboxOffset =
            new Vector3(1f, 0.8f, 0f);

        [Tooltip("Kích thước đầy đủ của hitbox, không phải half extents.")]
        [SerializeField]
        private Vector3 hitboxSize =
            new Vector3(1.5f, 1f, 1f);

        [Tooltip("Góc xoay cục bộ của hitbox. Với game side-view chủ yếu dùng trục Z.")]
        [SerializeField] private Vector3 hitboxEulerAngles;

        [Header("Animation")]
        [SerializeField, Min(0f)] private float crossFadeDuration = 0.05f;

        [Header("Movement")]
        [Tooltip("Khóa chuyển động ngang trong lúc thực hiện đòn này.")]
        [SerializeField] private bool lockHorizontalMovement = true;

        public string AttackName => attackName;
        public AnimActionKey AnimationKey => animationKey;

        public int Damage => damage;

        public Vector3 HitboxOffset => hitboxOffset;
        public Vector3 HitboxSize => hitboxSize;
        public Vector3 HitboxHalfExtents => hitboxSize * 0.5f;
        public Vector3 HitboxEulerAngles => hitboxEulerAngles;

        public float CrossFadeDuration => crossFadeDuration;
        public bool LockHorizontalMovement => lockHorizontalMovement;

#if UNITY_EDITOR
        private void OnValidate()
        {
            damage = Mathf.Max(0, damage);
            crossFadeDuration = Mathf.Max(0f, crossFadeDuration);

            hitboxSize.x = Mathf.Max(0.01f, hitboxSize.x);
            hitboxSize.y = Mathf.Max(0.01f, hitboxSize.y);
            hitboxSize.z = Mathf.Max(0.01f, hitboxSize.z);
        }
#endif
    }
}