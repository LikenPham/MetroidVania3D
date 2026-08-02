using System;
using System.Collections.Generic;

namespace Core.Events
{
    public static class EventManager
    {
        // Từ điển lưu trữ các sự kiện dựa trên kiểu dữ liệu Struct.
        private static readonly Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Đăng ký lắng nghe sự kiện
        /// </summary>
        public static void AddListener<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (!eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] = listener;
            }
            else
            {
                // Ép kiểu an toàn và gộp (Combine) delegate mới vào danh sách chờ
                eventTable[eventType] = Delegate.Combine(eventTable[eventType], listener);
            }
        }

        /// <summary>
        /// Hủy đăng ký lắng nghe sự kiện (Bắt buộc gọi ở OnDisable hoặc OnDestroy)
        /// </summary>
        public static void RemoveListener<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (eventTable.ContainsKey(eventType))
            {
                Delegate currentDelegate = eventTable[eventType];
                currentDelegate = Delegate.Remove(currentDelegate, listener);

                // Nếu không còn ai lắng nghe sự kiện này nữa, xóa luôn Key khỏi Dictionary cho nhẹ
                if (currentDelegate == null)
                {
                    eventTable.Remove(eventType);
                }
                else
                {
                    eventTable[eventType] = currentDelegate;
                }
            }
        }

        /// <summary>
        /// Phát sóng sự kiện ra toàn bộ game
        /// </summary>
        public static void Broadcast<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);

            if (eventTable.TryGetValue(eventType, out Delegate d))
            {
                // Ép kiểu ngược lại về Action<T> và kích hoạt
                if (d is Action<T> action)
                {
                    action.Invoke(eventData);
                }
            }
        }

        /// <summary>
        /// Dọn dẹp toàn bộ sự kiện. Hữu ích khi quay về Main Menu hoặc Reset toàn bộ Game.
        /// </summary>
        public static void ClearAll()
        {
            eventTable.Clear();
        }
    }
}