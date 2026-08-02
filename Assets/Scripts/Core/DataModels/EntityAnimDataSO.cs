using System.Collections.Generic;
using UnityEngine;

namespace Core.DataModels
{
    [System.Serializable]
    public struct AnimationMapping
    {
        public AnimActionKey ActionKey;       // Khóa định danh: "Idle", "Run", "Fly", "Swim", "Attack"
        public string AnimStateName;   // Tên thực tế trong Animator: "Fish_Swim", "Bird_Fly"
    }

    [CreateAssetMenu(fileName = "EntityAnimData", menuName = "Data/Animation Config")]
    public class EntityAnimDataSO : ScriptableObject
    {
        [SerializeField] private List<AnimationMapping> animationList;

        // Từ điển trong RAM để truy xuất tốc độ cao (O(1)), tránh duyệt vòng lặp List
        private readonly Dictionary<AnimActionKey, int> animHashDictionary = new();

        // Hàm này sẽ được gọi 1 lần duy nhất khi game khởi động để băm toàn bộ chuỗi thành số
        public void InitializeHashes()
        {
            animHashDictionary.Clear();

            foreach (var mapping in animationList)
            {
                if (mapping.ActionKey == AnimActionKey.None)
                    continue;

                if (string.IsNullOrEmpty(mapping.AnimStateName)) 
                    continue;

                int hash = Animator.StringToHash(mapping.AnimStateName);
                animHashDictionary[mapping.ActionKey] = hash;
            }
        }

        // Hàm lấy mã băm dựa vào Khóa định danh
        public int GetHash(AnimActionKey actionKey)
        {
            if (animHashDictionary.TryGetValue(actionKey, out int hash))
            {
                return hash;
            }

            Debug.LogWarning($"{name}: Missing animation hash for key {actionKey}.", this);
            return 0; // Trả về 0 nếu không tìm thấy hoạt ảnh (Lập trình phòng thủ)
        }
    }
}