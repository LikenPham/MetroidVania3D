using Core.DataModels;
using Core.Events;
using Core.Interfaces;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Modules
{
    public class HealthModule : MonoBehaviour, IDamageable
    {
        [Header("Standalone Setup (Dành cho Thùng gỗ/Rương)")]
        [Tooltip("Nếu quái/player có não bộ, để trống ô này. Não bộ sẽ tự gọi hàm Setup().")]
        [SerializeField] private HealthStatSO standaloneStat;
        [SerializeField] private string entityID = "Destructible_Object";

        [Header("Death Event")]
        [SerializeField] private bool broadcastEnemyDefeatedEvent = true;
        [SerializeField] private bool deactivateOnDeath = true;

        [Header("Visual/Audio Feedback (For Designer)")]
        [SerializeField] private UnityEvent OnTakeDamage;
        [SerializeField] private UnityEvent OnZeroHealth;

        // Biến lưu trữ dữ liệu thực tế sẽ dùng
        private HealthStatSO activeStat;

        // Biến trạng thái nội bộ
        private int currentHealth;
        private bool isDead;
        private bool isSetupComplete; // Cờ an toàn

        public int CurrentHealth => currentHealth;
        public int MaxHealth => activeStat != null ? activeStat.MaxHealth : 0;
        public bool IsDead => isDead;
        public bool IsSetupComplete => isSetupComplete;

        public event Action<DamageInfo> Damaged;
        public event Action<int, int> HealthChanged;
        public event Action Dead;
        private void Start()
        {
            // Nếu có gắn file trực tiếp trên Inspector (Thùng gỗ), tự Setup cho chính mình luôn
            if (standaloneStat != null && !isSetupComplete)
            {
                Setup(standaloneStat, entityID);
            }
        }

        // Não bộ (PlayerController) sẽ gọi hàm này từ Awake/Start của nó
        public void Setup(HealthStatSO healthData, string newEntityID)
        {
            if (healthData == null)
            {
                Debug.LogError($"{name}: HealthStatSO is null.", this);
                enabled = false;
                return;
            }

            activeStat = healthData;
            entityID = newEntityID;

            currentHealth = activeStat.MaxHealth;

            isDead = false;
            isSetupComplete = true; // Bật cờ an toàn

            BroadcastHealthChanged();
        }

        // ===============================================
        // THỰC THI INTERFACE
        // ===============================================
        public bool TakeDamage(in DamageInfo damageInfo)
        {
            // Tránh bug: Nếu chết rồi, hoặc chưa kịp Setup xong đã bị đạn bay trúng -> Từ chối sát thương
            if (isDead || !isSetupComplete) return false;

            if (damageInfo.AmountDamage <= 0)
                return false;

            // Trừ máu (Dùng .amount theo đúng chuẩn struct cũ)
            currentHealth -= damageInfo.AmountDamage;
            currentHealth = Mathf.Max(0, currentHealth);

            // 1. Kích hoạt hiệu ứng cục bộ Inspector
            OnTakeDamage?.Invoke();
            Damaged?.Invoke(damageInfo);

            // 2. Báo cáo vĩ mô cho UI
            BroadcastHealthChanged();

            // 3. Kiểm tra sinh tử
            if (currentHealth <= 0)
            {
                Die();
            }

            return true;
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;

            // Kích hoạt rớt đồ/nổ tung
            OnZeroHealth?.Invoke();
            Dead?.Invoke();

            // Báo cáo vĩ mô: "Tôi đã ngỏm, ai nhận quest thì cộng điểm đi"
            if (broadcastEnemyDefeatedEvent)
            {
                EventManager.Broadcast(new EnemyDefeatedEvent
                {
                    enemyID = entityID,
                    deathPosition = transform.position
                });
            }

            // Tái chế
            if (deactivateOnDeath)
            {
                gameObject.SetActive(false);
            }
        }

        private void BroadcastHealthChanged()
        {
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}