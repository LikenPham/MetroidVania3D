// Không cần 'using UnityEngine;' ở đây, giúp code sạch và giảm phụ thuộc (dependency)

namespace Core.Events
{
    // Còi báo hiệu đổi ngôn ngữ (Hệ thống Localization sẽ tự động lắng nghe)
    public struct LanguageChangedEvent
    {
    }

    // Báo hiệu chuyển Map (GameManager/LevelManager sẽ lo việc load Scene)
    public struct SceneTransitionEvent
    {
        public string targetSceneName;
        public string targetSpawnPointID;
    }
}