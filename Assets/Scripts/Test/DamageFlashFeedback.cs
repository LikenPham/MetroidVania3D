using System.Collections;
using Core.Interfaces;
using Gameplay.Modules;
using UnityEngine;

namespace Gameplay.Combat
{
    [DisallowMultipleComponent]
    public sealed class DamageFlashFeedback : MonoBehaviour
    {
        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");

        private static readonly int ColorId =
            Shader.PropertyToID("_Color");

        [Header("References")]
        [SerializeField] private HealthModule healthModule;
        [SerializeField] private Renderer targetRenderer;

        [Header("Flash")]
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField, Min(0.01f)] private float flashDuration = 0.08f;

        private MaterialPropertyBlock propertyBlock;
        private WaitForSeconds flashWait;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (healthModule == null)
            {
                healthModule = GetComponent<HealthModule>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            propertyBlock = new MaterialPropertyBlock();
            flashWait = new WaitForSeconds(flashDuration);
        }

        private void OnEnable()
        {
            if (healthModule != null)
            {
                healthModule.Damaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (healthModule != null)
            {
                healthModule.Damaged -= HandleDamaged;
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (targetRenderer == null)
                return;

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetFlashColor();

            yield return flashWait;

            ClearFlashColor();

            flashRoutine = null;
        }

        private void SetFlashColor()
        {
            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetColor(BaseColorId, flashColor);
            propertyBlock.SetColor(ColorId, flashColor);

            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ClearFlashColor()
        {
            propertyBlock.Clear();
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}