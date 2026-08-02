using UnityEngine;

namespace Core.DataModels
{
    [CreateAssetMenu(fileName = "NewHealthStat", menuName = "Metroidvania/DataModels/Stats/Health")]
    public class HealthStatSO : ScriptableObject
    {
        [Header("Health Core")]
        [Min(1)]
        [SerializeField] private int maxHealth = 100;

        // Getter chỉ đọc
        public int MaxHealth => maxHealth;
    }
}