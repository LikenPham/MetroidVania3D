using UnityEngine;

namespace Gameplay.Combat
{
    public sealed class DamageHitVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particle;

        private void Awake()
        {
            if (particle == null)
                particle = GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            if (particle != null)
                particle.Play();
        }
    }
}