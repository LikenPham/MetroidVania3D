using UnityEngine;

public interface IWallSensor
{
    bool IsTouchingRightWall { get; }
    bool IsTouchingLeftWall { get; }
    Vector3 RightWallNormal { get; }
    Vector3 LeftWallNormal { get; }
}
