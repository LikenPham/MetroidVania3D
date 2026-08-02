using UnityEngine;

namespace Core.Events
{
    // Còi báo hiệu máu thay đổi (Dùng cho UI, cảnh báo đỏ màn hình...)
    public struct HealthChangedEvent
    {
        public int currentHealth;
        public int maxHealth;
    }

    // Báo hiệu quái chết (Dùng cho Quest, rớt đồ, cộng điểm...)
    public struct EnemyDefeatedEvent
    {
        public string enemyID;
        public Vector3 deathPosition;
    }
}