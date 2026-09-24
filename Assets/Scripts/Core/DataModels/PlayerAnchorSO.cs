using UnityEngine;

namespace Core.DataModels
{
    [CreateAssetMenu(
        fileName = "PlayerAnchor",
        menuName = "Data/Player Anchor"
    )]
    public sealed class PlayerAnchorSO : ScriptableObject
    {
        public Transform PlayerTransform { get; private set; }

        public void SetPlayer(Transform player)
        {
            PlayerTransform = player;
        }

        public void ClearPlayer(Transform player)
        {
            if (PlayerTransform == player)
                PlayerTransform = null;
        }
    }
}