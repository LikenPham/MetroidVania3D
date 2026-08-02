using Gameplay.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHealthUI : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private HealthModule playerHealth;

        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TMP_Text healthText;

        private bool isSubscribed;

        private void Awake()
        {
            ValidateReferences();
            ResetUI();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Setup(HealthModule healthModule)
        {
            if (playerHealth == healthModule && isSubscribed)
            {
                RefreshFromHealthModule();
                return;
            }

            Unsubscribe();

            playerHealth = healthModule;

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void ValidateReferences()
        {
            if (healthSlider == null)
            {
                Debug.LogError($"{name}: Health Slider chưa được gán.", this);
                enabled = false;
                return;
            }

            if (healthText == null)
            {
                Debug.LogError($"{name}: Health Text chưa được gán.", this);
                enabled = false;
            }
        }

        private void Subscribe()
        {
            if (isSubscribed)
                return;

            if (playerHealth == null)
            {
                Debug.LogWarning($"{name}: Player HealthModule chưa được gán.", this);
                ResetUI();
                return;
            }

            playerHealth.HealthChanged += HandleHealthChanged;
            isSubscribed = true;

            RefreshFromHealthModule();
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
                return;

            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= HandleHealthChanged;
            }

            isSubscribed = false;
        }

        private void RefreshFromHealthModule()
        {
            if (playerHealth == null)
            {
                ResetUI();
                return;
            }

            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            int safeMaxHealth = Mathf.Max(1, maxHealth);
            int safeCurrentHealth = Mathf.Clamp(currentHealth, 0, safeMaxHealth);

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = safeMaxHealth;
                healthSlider.value = safeCurrentHealth;
            }

            if (healthText != null)
            {
                healthText.SetText("{0}/{1}", safeCurrentHealth, safeMaxHealth);
            }
        }

        private void ResetUI()
        {
            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = 1f;
                healthSlider.value = 0f;
            }

            if (healthText != null)
            {
                healthText.SetText("0/0");
            }
        }
    }
}