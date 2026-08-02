using UnityEngine;

public sealed class FrameRateDebug : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"VSync Count: {QualitySettings.vSyncCount}");
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
        Debug.Log($"Screen Refresh Rate: {Screen.currentResolution.refreshRateRatio.value:F2} Hz");
    }
}