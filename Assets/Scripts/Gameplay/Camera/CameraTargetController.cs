using Core.Events;
using Core.Interfaces;
using UnityEngine;

namespace Gameplay.Camera
{
    public class CameraTargetController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LayerMask groundLayer;

        [Header("Horizontal Lookahead Bias")]
        [SerializeField] private float lookAheadDistance = 2f; // Liếc về trước mặt 2 mét
        [SerializeField] private float lookAheadSpeed = 3f;    // Tốc độ lướt "con ngươi" khi lật mặt

        [Header("Asymmetric Framing Window")]
        [SerializeField] private float jumpUpThreshold = 3.5f;
        [SerializeField] private float fallDownThreshold = 1.5f;

        [Header("Dynamic Vertical Damping")]
        [SerializeField] private float normalDampingY = 6f;    // Tốc độ cuộn mượt khi đi ngang / nhảy lên
        [SerializeField] private float fastFallDampingY = 12f; // Tốc độ cuộn "chớp nhoáng" khi rớt hố

        [SerializeField] private MonoBehaviour groundSensorSource;

        private IGroundSensor groundSensor;
        private float targetY;
        private float lockedSurfaceY;

        private float currentLookAheadX; // Lưu trữ vị trí "con ngươi" hiện tại

        private void Awake()
        {
            // Ép kiểu an toàn ngay từ mốc 0 giây
            groundSensor = groundSensorSource as IGroundSensor;

            if (groundSensor == null && groundSensorSource != null)
            {
                Debug.LogError($"[CameraTarget] Vật thể {groundSensorSource.name} không ký hợp đồng IGroundSensor!");
            }
        }

        private void Start()
        {
            if (playerTransform != null)
            {
                lockedSurfaceY = playerTransform.position.y;
                targetY = lockedSurfaceY;
                currentLookAheadX = GetTargetLookAhead();
            }
        }

        private float FindTrueSurfaceY(float fallbackY)
        {
            if (playerTransform == null) return fallbackY;
            Vector3 origin = new Vector3(playerTransform.position.x, playerTransform.position.y + 1f, playerTransform.position.z);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2f, groundLayer)) return hit.point.y;
            return fallbackY;
        }

        // PHÉP THUẬT: Đọc hướng mặt của Player mà KHÔNG CẦN reference tới PlayerStateMachine!
        private float GetTargetLookAhead()
        {
            // Trong Hiến pháp phần II: Khi Player lật mặt, ta xoay Y 180 độ.
            // Khi xoay Y 180 độ, vector transform.right.x tự động biến thành số Âm! 
            bool isFacingRight = playerTransform.right.x > 0f;
            return isFacingRight ? lookAheadDistance : -lookAheadDistance;
        }

        private void LateUpdate()
        {
            if (playerTransform == null || groundSensor == null) return;

            // ================================================================
            // CHIẾN TRƯỜNG 1: TRỤC X (MẮT LIẾC MƯỢT MÀ)
            // ================================================================
            float targetLookAheadX = GetTargetLookAhead();

            // Dùng Lerp để con ngươi lướt qua lướt lại mượt mà khi bấm A/D liên tục
            currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadSpeed);

            // Tọa độ X cuối cùng = Gót chân Player + Độ liếc
            float nextX = playerTransform.position.x + currentLookAheadX;

            // ================================================================
            // CHIẾN TRƯỜNG 2: TRỤC Y (HỘP SỐ BIẾN THIÊN KHI RƠI)
            // ================================================================
            
            float activeDampingY = normalDampingY; // Mặc định gài số chậm
            if (groundSensor.IsGrounded)
            {
                // [CHẾ ĐỘ MẶT ĐẤT] (Đi bộ trên sàn, chạy dốc, xuống cầu thang)
                // Vô hiệu hóa toàn bộ lồng Deadzone! Khóa mục tiêu thẳng vào mặt sàn thực tế.
                lockedSurfaceY = FindTrueSurfaceY(playerTransform.position.y);
                targetY = lockedSurfaceY;
            }
            else
            {
                // [CHẾ ĐỘ TRÊN KHÔNG] (Đang bay nhảy hoặc rơi tự do)
                // Kích hoạt lồng Deadzone bất đối xứng để không giật màn hình
                float diffY = playerTransform.position.y - targetY;
                // Luồng A: Nhảy kịch trần
                if (diffY > jumpUpThreshold)
                {
                    targetY = playerTransform.position.y - jumpUpThreshold;
                }
                // Luồng B: Rớt hố -> GÀI SỐ BỐC ĐẦU!
                else if (diffY < -fallDownThreshold)
                {
                    targetY = playerTransform.position.y + fallDownThreshold;
                    activeDampingY = fastFallDampingY; // KÍCH HOẠT: Ép camera lao thẳng xuống dưới tốc độ 12f!
                }

            }

            // Chốt chặn bê tông sàn nhà
            if (playerTransform.position.y >= lockedSurfaceY - 0.1f)
            {
                targetY = Mathf.Max(targetY, lockedSurfaceY);
            }

            // ================================================================
            // TÍNH TOÁN HỆ SỐ LERP ĐỘC LẬP TẦN SỐ KHUNG HÌNH (Frame-Rate Independent)
            // ================================================================
            // lerpFactor luôn tiệm cận về một tỷ lệ hằng số thời gian, giúp máy 30 FPS và 120 FPS trượt mượt như nhau
            float lerpFactor = 1f - Mathf.Exp(-activeDampingY * Time.deltaTime);
            float nextY = Mathf.Lerp(transform.position.y, targetY, lerpFactor);

            transform.position = new Vector3(nextX, nextY, transform.position.z);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && playerTransform != null)
            {
                transform.position = playerTransform.position;
            }
        }

        private void OnValidate()
        {
            if (groundSensorSource != null && !(groundSensorSource is IGroundSensor))
            {
                // Phát hiện kéo nhầm một MonoBehaviour không ký hợp đồng IGroundSensor (VD: MoveModule)
                // -> Code lập tức đi vòng quanh GameObject đó tìm xem có anh chị em nào ký hợp đồng không!
                if (groundSensorSource.TryGetComponent<IGroundSensor>(out var correctSensor))
                {
                    groundSensorSource = correctSensor as MonoBehaviour;
                    Debug.Log("<i>[Camera] Đã tự động sửa nhầm lẫn: Ép nhận lại CharacterSensor!</i>");
                }
                else
                {
                    Debug.LogError($"[Camera] Object '{groundSensorSource.name}' không có script nào ký hợp đồng IGroundSensor cả!");
                    groundSensorSource = null; // Tát vào tay Designer: Tự động xóa ô đó về None!
                }
            }
        }
#endif
    }
}