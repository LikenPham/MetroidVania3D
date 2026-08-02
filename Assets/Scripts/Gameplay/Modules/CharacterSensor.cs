using Core.Interfaces;
using UnityEngine;

public class CharacterSensor : MonoBehaviour, ISurfaceSensor, IWallSensor
{
    [Header("2.5D Axis Config")]
    [SerializeField] private Vector3 horizontalAxis = Vector3.right;
    [SerializeField] private Vector3 verticalAxis = Vector3.up;
    [SerializeField] private Vector3 depthAxis = Vector3.forward;
    [SerializeField] private bool constrainTo2DPlane = true;
    [SerializeField] private float maxDepthOffset = 0.15f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private float groundCastUpOffset = 0.2f;
    [SerializeField] private float groundCastDistance = 0.45f;
    [SerializeField] private float groundRayExtraDistance = 0.1f;
    [SerializeField] private float groundRayOffsetMultiplier = 0.55f;
    [SerializeField] private float maxWalkableSlopeAngle = 45f;
    [SerializeField] private float slopeEpsilon = 0.1f;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckDistance = 0.4f;
    [SerializeField] private float wallBottomOffset = 0.25f;
    [SerializeField] private float wallTopOffset = 1.5f;
    [SerializeField] private float wallMinAngle = 70f;
    [SerializeField] private float wallFacingDotThreshold = 0.45f;

    [Header("Layer")]
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    public bool IsOnSlope { get; private set; }
    public bool IsTouchingRightWall { get; private set; }
    public bool IsTouchingLeftWall { get; private set; }

    public Vector3 SurfaceNormal { get; private set; } = Vector3.up;
    public Vector3 RightWallNormal { get; private set; } = Vector3.left;
    public Vector3 LeftWallNormal { get; private set; } = Vector3.right;

    private Vector3 Horizontal => horizontalAxis.normalized;
    private Vector3 Vertical => verticalAxis.normalized;
    private Vector3 Depth => depthAxis.normalized;

    private void FixedUpdate()
    {
        EvaluateGroundAndSlope();
        EvaluateWalls();
    }

    private void EvaluateGroundAndSlope()
    {
        ResetGroundState();

        if (groundCheckPoint == null)
            return;

        Vector3 up = Vertical;
        Vector3 down = -Vertical;

        float castRadius = groundRadius * 0.8f;
        Vector3 castOrigin = groundCheckPoint.position + up * groundCastUpOffset;

        bool hasSphereHit = Physics.SphereCast(
            castOrigin,
            castRadius,
            down,
            out RaycastHit sphereHit,
            groundCastDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hasSphereHit)
            return;

        if (!IsHitInsideGameplayPlane(sphereHit.point))
            return;

        if (!IsWalkableGround(sphereHit.normal))
            return;

        float rayLength = groundCastUpOffset + groundCastDistance + castRadius + groundRayExtraDistance;

        bool hasConfirmedGround = TryConfirmGroundWith2DRayFan(
            castOrigin,
            down,
            rayLength,
            out RaycastHit confirmedHit
        );

        if (!hasConfirmedGround)
            return;

        float groundAngle = Vector3.Angle(confirmedHit.normal, up);

        IsGrounded = true;
        IsOnSlope = groundAngle > slopeEpsilon;
        SurfaceNormal = confirmedHit.normal;
    }

    private bool TryConfirmGroundWith2DRayFan(
    Vector3 centerOrigin,
    Vector3 down,
    float rayLength,
    out RaycastHit bestHit)
    {
        bestHit = default;

        float offset = groundRadius * groundRayOffsetMultiplier;

        Vector3 rightOrigin = centerOrigin + Horizontal * offset;
        Vector3 leftOrigin = centerOrigin - Horizontal * offset;

        bool foundGround = false;

        TryChooseBestGroundRay(centerOrigin, down, rayLength, ref foundGround, ref bestHit);
        TryChooseBestGroundRay(rightOrigin, down, rayLength, ref foundGround, ref bestHit);
        TryChooseBestGroundRay(leftOrigin, down, rayLength, ref foundGround, ref bestHit);

        return foundGround;
    }

    private bool TryGetGroundRay(
    Vector3 origin,
    Vector3 direction,
    float rayLength,
    out RaycastHit hit)
    {
        bool hasHit = Physics.Raycast(
            origin,
            direction,
            out hit,
            rayLength,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hasHit)
            return false;

        if (!IsHitInsideGameplayPlane(hit.point))
            return false;

        if (!IsWalkableGround(hit.normal))
            return false;

        return true;
    }

    private void TryChooseBestGroundRay(
    Vector3 origin,
    Vector3 direction,
    float rayLength,
    ref bool foundGround,
    ref RaycastHit bestHit)
    {
        if (!TryGetGroundRay(origin, direction, rayLength, out RaycastHit hit))
            return;

        if (!foundGround || hit.distance < bestHit.distance)
        {
            foundGround = true;
            bestHit = hit;
        }
    }

    private bool IsWalkableGround(Vector3 normal)
    {
        float angle = Vector3.Angle(normal, Vertical);
        return angle <= maxWalkableSlopeAngle;
    }

    private bool IsHitInsideGameplayPlane(Vector3 hitPoint)
    {
        if (!constrainTo2DPlane)
            return true;

        if (groundCheckPoint == null)
            return true;

        Vector3 fromGroundPointToHit = hitPoint - groundCheckPoint.position;
        float depthOffset = Mathf.Abs(Vector3.Dot(fromGroundPointToHit, Depth));

        return depthOffset <= maxDepthOffset;
    }

    private void EvaluateWalls()
    {
        Vector3 pBottom = transform.position + Vertical * wallBottomOffset;
        Vector3 pTop = transform.position + Vertical * wallTopOffset;

        float castRadius = groundRadius * 0.9f;

        IsTouchingRightWall = CastWallCapsule(
            pBottom,
            pTop,
            castRadius,
            Horizontal,
            out RaycastHit hitRight
        );

        RightWallNormal = IsTouchingRightWall ? hitRight.normal : -Horizontal;

        IsTouchingLeftWall = CastWallCapsule(
            pBottom,
            pTop,
            castRadius,
            -Horizontal,
            out RaycastHit hitLeft
        );

        LeftWallNormal = IsTouchingLeftWall ? hitLeft.normal : Horizontal;
    }

    private bool CastWallCapsule(
        Vector3 p1,
        Vector3 p2,
        float radius,
        Vector3 direction,
        out RaycastHit hit)
    {
        bool hasHit = Physics.CapsuleCast(
            p1,
            p2,
            radius,
            direction,
            out hit,
            wallCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!hasHit)
            return false;

        if (!IsHitInsideGameplayPlane(hit.point))
            return false;

        float angleFromUp = Vector3.Angle(hit.normal, Vertical);

        if (angleFromUp < wallMinAngle)
            return false;

        // Nếu cast sang phải, normal của tường phải hướng ngược lại, tức là về bên trái.
        // Nếu cast sang trái, normal của tường phải hướng ngược lại, tức là về bên phải.
        float facingDot = Vector3.Dot(hit.normal.normalized, -direction.normalized);

        return facingDot >= wallFacingDotThreshold;
    }

    private void ResetGroundState()
    {
        IsGrounded = false;
        IsOnSlope = false;
        SurfaceNormal = Vertical;
    }

    private void OnValidate()
    {
        if (horizontalAxis == Vector3.zero)
            horizontalAxis = Vector3.right;

        if (verticalAxis == Vector3.zero)
            verticalAxis = Vector3.up;

        if (depthAxis == Vector3.zero)
            depthAxis = Vector3.forward;

        groundRadius = Mathf.Max(0.01f, groundRadius);
        groundCastUpOffset = Mathf.Max(0.01f, groundCastUpOffset);
        groundCastDistance = Mathf.Max(0.01f, groundCastDistance);
        groundRayExtraDistance = Mathf.Max(0f, groundRayExtraDistance);
        groundRayOffsetMultiplier = Mathf.Clamp(groundRayOffsetMultiplier, 0f, 1f);

        maxWalkableSlopeAngle = Mathf.Clamp(maxWalkableSlopeAngle, 0f, 89f);
        slopeEpsilon = Mathf.Max(0f, slopeEpsilon);

        wallCheckDistance = Mathf.Max(0.01f, wallCheckDistance);
        wallBottomOffset = Mathf.Max(0f, wallBottomOffset);
        wallTopOffset = Mathf.Max(wallBottomOffset + 0.01f, wallTopOffset);
        wallMinAngle = Mathf.Clamp(wallMinAngle, 0f, 180f);
        wallFacingDotThreshold = Mathf.Clamp01(wallFacingDotThreshold);

        maxDepthOffset = Mathf.Max(0f, maxDepthOffset);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 horizontal = horizontalAxis == Vector3.zero ? Vector3.right : horizontalAxis.normalized;
        Vector3 vertical = verticalAxis == Vector3.zero ? Vector3.up : verticalAxis.normalized;

        if (groundCheckPoint != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundRadius);

            Vector3 castOrigin = groundCheckPoint.position + vertical * groundCastUpOffset;
            float castRadius = groundRadius * 0.8f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(castOrigin, castRadius);
            Gizmos.DrawLine(castOrigin, castOrigin - vertical * groundCastDistance);

            float rayLength = groundCastUpOffset + groundCastDistance + castRadius + groundRayExtraDistance;
            float offset = groundRadius * groundRayOffsetMultiplier;

            Gizmos.color = Color.cyan;
            DrawRayGizmo(castOrigin, -vertical, rayLength);
            DrawRayGizmo(castOrigin + horizontal * offset, -vertical, rayLength);
            DrawRayGizmo(castOrigin - horizontal * offset, -vertical, rayLength);
        }

        Vector3 pBottom = transform.position + vertical * wallBottomOffset;
        Vector3 pTop = transform.position + vertical * wallTopOffset;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pBottom, pBottom + horizontal * wallCheckDistance);
        Gizmos.DrawLine(pTop, pTop + horizontal * wallCheckDistance);
        Gizmos.DrawLine(pBottom, pBottom - horizontal * wallCheckDistance);
        Gizmos.DrawLine(pTop, pTop - horizontal * wallCheckDistance);
    }

    private void DrawRayGizmo(Vector3 origin, Vector3 direction, float length)
    {
        Gizmos.DrawLine(origin, origin + direction.normalized * length);
    }
}