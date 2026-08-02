using UnityEngine;

namespace Core.DataModels
{
    public class EntityDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string entityID = "Unique_ID";
        [SerializeField] private string displayName = "Unknown Entity";

        // Getter chỉ đọc
        public string EntityID => entityID;
        public string DisplayName => displayName;
    }
}