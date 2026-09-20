using UnityEngine;

namespace Gameplay.Enemy
{
    public sealed class EnemyMovementModule : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2f;

        [Header("Visual")]
        [SerializeField] private Transform visualRoot;

        [SerializeField] private float rightFacingAngle = 90f;
        [SerializeField] private float leftFacingAngle = -90f;

        private void Awake()
        {
            if (visualRoot == null)
                visualRoot = transform;
        }

        public void Move(Vector3 direction)
        {
            Vector3 movement = direction.normalized * moveSpeed;

            transform.position += movement * Time.deltaTime;

            FaceDirection(direction);
        }

        public void FaceDirection(Vector3 direction)
        {
            if (direction.x > 0f)
            {
                visualRoot.rotation =
                    Quaternion.Euler(0f, rightFacingAngle, 0f);
            }
            else if (direction.x < 0f)
            {
                visualRoot.rotation =
                    Quaternion.Euler(0f, leftFacingAngle, 0f);
            }
        }
    }
}