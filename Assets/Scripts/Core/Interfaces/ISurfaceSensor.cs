using UnityEngine;
namespace Core.Interfaces
{
    public interface ISurfaceSensor : IGroundSensor
    {
        bool IsOnSlope { get; }
        Vector3 SurfaceNormal { get; }
    }
}