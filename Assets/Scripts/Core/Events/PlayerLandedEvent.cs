namespace Core.Events
{
    // Gói tin này sẽ bay trong không trung khi Player chạm đất
    public readonly struct PlayerLandedEvent
    {
        public readonly float LandPositionY;

        public PlayerLandedEvent(float yPosition)
        {
            LandPositionY = yPosition;
        }
    }
}