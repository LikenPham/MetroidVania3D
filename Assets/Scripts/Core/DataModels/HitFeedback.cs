using UnityEngine;

namespace Core.DataModels
{
    public readonly struct HitFeedback
    {
        public readonly Vector3 HitPoint;
        public readonly Vector3 Direction;

        public HitFeedback(Vector3 hitPoint, Vector3 direction)
        {
            HitPoint = hitPoint;
            Direction = direction;
        }
    }
}
