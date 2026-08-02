using TMPro;
using UnityEngine;

public sealed class FpsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;

    private float _timer;
    private int _frames;

    private void Update()
    {
        _frames++;
        _timer += Time.unscaledDeltaTime;

        if (_timer < 0.5f)
            return;

        float fps = _frames / _timer;
        fpsText.SetText("FPS: {0:0}", fps);

        _frames = 0;
        _timer = 0f;
    }
}