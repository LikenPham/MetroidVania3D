using UnityEngine;

namespace Gameplay.Enemy
{
    [DisallowMultipleComponent]
    public sealed class EnemyWallSensor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Transform sensorOrigin;
        [SerializeField] private float checkDistance = 0.2f;
        [SerializeField] private float checkRadius = 0.15f;
        [SerializeField] private LayerMask obstacleLayers;

        private readonly RaycastHit[] hitResults = new RaycastHit[4];

        public bool IsWallAhead { get; private set; }

        private void Awake()
        {
            if (sensorOrigin == null)
            {
                sensorOrigin = transform;
            }
        }

        public void Check(bool isFacingRight)
        {
            Vector3 direction = isFacingRight
                ? Vector3.right
                : Vector3.left;

            int hitCount = Physics.SphereCastNonAlloc(
                sensorOrigin.position,
                checkRadius,
                direction,
                hitResults,
                checkDistance,
                obstacleLayers,
                QueryTriggerInteraction.Ignore
            );

            IsWallAhead = hitCount > 0;
        }
    }
}