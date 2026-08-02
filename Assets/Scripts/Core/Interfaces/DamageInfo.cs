using UnityEngine;

namespace Core.Interfaces
{
    public readonly struct DamageInfo
    {
        public readonly int AmountDamage;             // Lượng sát thương
        public readonly Vector3 HitPoint;       // Vị trí va chạm (Dùng để sinh Particle xẹt lửa ngay vị trí chém)
        public readonly Vector3 AttackSource;   // Tọa độ kẻ ra đòn (Dùng để tính hướng Knockback đẩy lùi)
        // Sau này có thể thêm: public DamageType element; (Lửa, Băng, Độc...)

        public DamageInfo(int amountDamage, Vector3 hitPoint, Vector3 attackSource)
        {
            AmountDamage = amountDamage;
            HitPoint = hitPoint;
            AttackSource = attackSource;
        }
    }
}