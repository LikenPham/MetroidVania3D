namespace Core.Interfaces
{
    public interface IDamageable
    {
        /// <summary>
        /// Xử lý khi nhận sát thương từ tác nhân bên ngoài.
        /// Trả về true nếu sát thương thực sự được áp dụng (Không bị né/Bất tử).
        /// </summary>
        bool TakeDamage(in DamageInfo damageInfo);
    }
}