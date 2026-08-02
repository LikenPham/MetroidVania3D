using UnityEngine;

namespace Core.DataModels
{
    [CreateAssetMenu(fileName = "NewPlayerStat", menuName = "Metroidvania/DataModels/Entities/Player")]
    public class PlayerStatSO : EntityDataSO
    {
        [Header("Composed Stats (Read-Only)")]
        [SerializeField] private HealthStatSO healthStats;
        [SerializeField] private MovementStatSO movementStats;

        // Getter trả về nguyên một cái Thẻ nhớ nhỏ để truyền cho các Module
        public HealthStatSO HealthStats => healthStats;
        public MovementStatSO MovementStats => movementStats;
    }
}